// ========================================
// Archivo: Arsmate.Infrastructure/Services/PaymentService.cs
// ========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using Arsmate.Core.DTOs.Payment;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Entities;
using Arsmate.Core.Enums;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;


using RefundReason = Stripe.RefundReasons;
using ChargeService = Stripe.ChargeService;
using Charge = Stripe.Charge;
using ChargeGetOptions = Stripe.ChargeGetOptions;

namespace Arsmate.Infrastructure.Services
{
    /// <summary>
    /// Servicio completo de pagos con integración de Stripe
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly ArsmateDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<PaymentService> _logger;

        // Stripe services
        private readonly CustomerService _stripeCustomerService;
        private readonly ChargeService _stripeChargeService;
        private readonly PaymentIntentService _stripePaymentIntentService;
        private readonly RefundService _stripeRefundService;
        private readonly AccountService _stripeAccountService;
        private readonly TransferService _stripeTransferService;
        private readonly BalanceService _stripeBalanceService;
        private readonly PayoutService _stripePayoutService;
        private readonly SessionService _stripeSessionService;

        private const decimal PLATFORM_FEE_PERCENTAGE = 0.15m; // 15% platform fee
        private const decimal WITHDRAWAL_FEE_PERCENTAGE = 0.025m; // 2.5% withdrawal fee
        private const decimal MINIMUM_WITHDRAWAL_AMOUNT = 20m;

        public PaymentService(
            ArsmateDbContext context,
            IConfiguration configuration,
            INotificationService notificationService,
            IEmailService emailService,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _configuration = configuration;
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;

            // Initialize Stripe
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            // Initialize Stripe services
            _stripeCustomerService = new CustomerService();
            _stripeChargeService = new ChargeService();
            _stripePaymentIntentService = new PaymentIntentService();
            _stripeRefundService = new RefundService();
            _stripeAccountService = new AccountService();
            _stripeTransferService = new TransferService();
            _stripeBalanceService = new BalanceService();
            _stripePayoutService = new PayoutService();
            _stripeSessionService = new SessionService();
        }

        /// <summary>
        /// Procesa un pago para suscripción, propina, PPV, etc.
        /// </summary>
        public async Task<PaymentDto> ProcessPaymentAsync(Guid userId, CreatePaymentDto createPaymentDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException("User not found");

                // Crear transacción en estado pendiente
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = createPaymentDto.Type,
                    Status = Arsmate.Core.Enums.TransactionStatus.Pending,
                    Amount = createPaymentDto.Amount,
                    Currency = createPaymentDto.Currency,
                    Description = createPaymentDto.Description,
                    PaymentMethod = createPaymentDto.PaymentMethodId,
                    SubscriptionId = createPaymentDto.SubscriptionId,
                    PostId = createPaymentDto.PostId,
                    MessageId = createPaymentDto.MessageId,
                    IpAddress = GetClientIpAddress(),
                    CountryCode = await GetCountryCodeFromIp(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                try
                {
                    // Obtener o crear cliente de Stripe
                    var stripeCustomerId = await GetOrCreateStripeCustomer(user);

                    // Crear PaymentIntent en Stripe
                    var paymentIntentOptions = new PaymentIntentCreateOptions
                    {
                        Amount = ConvertToStripAmount(createPaymentDto.Amount, createPaymentDto.Currency),
                        Currency = createPaymentDto.Currency.ToLower(),
                        Customer = stripeCustomerId,
                        PaymentMethod = createPaymentDto.PaymentMethodId,
                        Description = createPaymentDto.Description,
                        Metadata = new Dictionary<string, string>
                {
                    { "transaction_id", transaction.Id.ToString() },
                    { "user_id", userId.ToString() },
                    { "type", createPaymentDto.Type.ToString() }
                },
                        Confirm = true,
                        ReturnUrl = $"{_configuration["App:BaseUrl"]}/payment/success",
                        // Expandir los campos necesarios para obtener información de la tarjeta
                        Expand = new List<string> { "latest_charge", "latest_charge.payment_method_details" }
                    };

                    // Agregar información adicional según el tipo de pago
                    if (createPaymentDto.Type == TransactionType.Subscription && createPaymentDto.SubscriptionId.HasValue)
                    {
                        paymentIntentOptions.Metadata.Add("subscription_id", createPaymentDto.SubscriptionId.Value.ToString());
                    }
                    else if (createPaymentDto.Type == TransactionType.Tip && createPaymentDto.TipRecipientId.HasValue)
                    {
                        paymentIntentOptions.Metadata.Add("recipient_id", createPaymentDto.TipRecipientId.Value.ToString());
                    }

                    var paymentIntent = await _stripePaymentIntentService.CreateAsync(paymentIntentOptions);

                    // Verificar el estado del pago
                    if (paymentIntent.Status == "succeeded")
                    {
                        // Pago exitoso
                        transaction.Status = Arsmate.Core.Enums.TransactionStatus.Completed;
                        transaction.ExternalTransactionId = paymentIntent.Id;
                        transaction.ProcessedAt = DateTime.UtcNow;

                        // Calcular fees
                        transaction.Fee = transaction.Amount * PLATFORM_FEE_PERCENTAGE;
                        transaction.NetAmount = transaction.Amount - transaction.Fee.Value;

                        // Obtener últimos 4 dígitos de la tarjeta usando LatestCharge
                        if (paymentIntent.LatestCharge != null)
                        {
                            // Si necesitamos más detalles del cargo, podemos obtenerlo
                            var charge = await GetChargeDetailsAsync(paymentIntent.LatestChargeId);
                            if (charge?.PaymentMethodDetails?.Card != null)
                            {
                                transaction.CardLast4 = charge.PaymentMethodDetails.Card.Last4;
                            }
                        }

                        // Procesar según el tipo de transacción
                        await ProcessSuccessfulPayment(transaction, createPaymentDto);
                    }
                    else if (paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_source_action")
                    {
                        // Requiere autenticación 3D Secure
                        transaction.Status = Arsmate.Core.Enums.TransactionStatus.RequiresVerification;
                        transaction.ExternalTransactionId = paymentIntent.Id;

                        _logger.LogInformation($"Payment requires 3D Secure authentication: {transaction.Id}");
                    }
                    else
                    {
                        // Pago fallido
                        transaction.Status = Arsmate.Core.Enums.TransactionStatus.Failed;
                        transaction.FailedAt = DateTime.UtcNow;
                        transaction.FailureReason = $"Payment failed with status: {paymentIntent.Status}";

                        _logger.LogWarning($"Payment failed: {transaction.Id} - Status: {paymentIntent.Status}");
                    }
                }
                catch (StripeException ex)
                {
                    // Error de Stripe
                    transaction.Status = Arsmate.Core.Enums.TransactionStatus.Failed;
                    transaction.FailedAt = DateTime.UtcNow;
                    transaction.FailureReason = ex.Message;

                    _logger.LogError(ex, $"Stripe error processing payment: {transaction.Id}");
                    throw new InvalidOperationException($"Payment processing failed: {ex.Message}");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment processed: {transaction.Id} - Status: {transaction.Status}");

                return MapToPaymentDto(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                throw;
            }
        }

        /// <summary>
        /// Obtiene los detalles de un cargo de Stripe
        /// </summary>
        private async Task<Charge> GetChargeDetailsAsync(string chargeId)
        {
            try
            {
                if (string.IsNullOrEmpty(chargeId))
                    return null;

                // Obtener el cargo con los detalles expandidos
                var options = new ChargeGetOptions
                {
                    Expand = new List<string> { "payment_method_details" }
                };

                return await _stripeChargeService.GetAsync(chargeId, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Could not retrieve charge details for {chargeId}");
                return null;
            }
        }

        // TAMBIÉN CORREGIR EL MÉTODO RefundPaymentAsync:

        /// <summary>
        /// Procesa un reembolso
        /// </summary>
        public async Task<bool> RefundPaymentAsync(Guid paymentId, string reason)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(paymentId);
                if (transaction == null || transaction.Status != Arsmate.Core.Enums.TransactionStatus.Completed)
                    return false;

                if (string.IsNullOrEmpty(transaction.ExternalTransactionId))
                {
                    _logger.LogWarning($"Cannot refund payment without external transaction ID: {paymentId}");
                    return false;
                }

                // Crear reembolso en Stripe
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = transaction.ExternalTransactionId,
                    Amount = ConvertToStripAmount(transaction.Amount, transaction.Currency),
                    Reason = RefundReason.RequestedByCustomer, // Usar el enum correcto
                    Metadata = new Dictionary<string, string>
            {
                { "transaction_id", transaction.Id.ToString() },
                { "reason", reason }
            }
                };

                var refund = await _stripeRefundService.CreateAsync(refundOptions);

                if (refund.Status == "succeeded")
                {
                    // Actualizar transacción
                    transaction.Status = Arsmate.Core.Enums.TransactionStatus.Refunded;
                    transaction.UpdatedAt = DateTime.UtcNow;

                    // Revertir los cambios según el tipo de transacción
                    await RevertPaymentEffects(transaction);

                    // Crear transacción de reembolso
                    var refundTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = transaction.UserId,
                        Type = TransactionType.Refund,
                        Status = Arsmate.Core.Enums.TransactionStatus.Completed,
                        Amount = -transaction.Amount, // Negativo para reembolso
                        Currency = transaction.Currency,
                        Description = $"Refund for transaction {transaction.Id}: {reason}",
                        ExternalTransactionId = refund.Id,
                        ProcessedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.Transactions.AddAsync(refundTransaction);
                    await _context.SaveChangesAsync();

                    // Notificar al usuario
                    await _notificationService.CreateNotificationAsync(
                        transaction.UserId,
                        NotificationType.PaymentReceived,
                        "Refund Processed",
                        $"Your refund of {transaction.Amount} {transaction.Currency} has been processed.",
                        new { transactionId = transaction.Id, refundId = refund.Id }
                    );

                    _logger.LogInformation($"Payment refunded: {paymentId} - Refund ID: {refund.Id}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Refund failed with status: {refund.Status}");
                    return false;
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"Stripe error refunding payment: {paymentId}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error refunding payment: {paymentId}");
                return false;
            }
        }


        /// <summary>
        /// Obtiene un pago por su ID
        /// </summary>
        public async Task<PaymentDto> GetPaymentByIdAsync(Guid paymentId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == paymentId);

            if (transaction == null)
                return null;

            return MapToPaymentDto(transaction);
        }

        /// <summary>
        /// Obtiene los pagos de un usuario con paginación
        /// </summary>
        public async Task<PaginatedResultDto<PaymentDto>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();

            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToPaymentDto(t))
                .ToListAsync();

            return new PaginatedResultDto<PaymentDto>(payments, totalCount, page, pageSize);
        }

        /// <summary>
        /// Obtiene la billetera de un usuario
        /// </summary>
        public async Task<WalletDto> GetWalletAsync(Guid userId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                // Crear billetera si no existe
                wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Balance = 0,
                    PendingBalance = 0,
                    Currency = "USD",
                    MinimumWithdrawalAmount = MINIMUM_WITHDRAWAL_AMOUNT,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Wallets.AddAsync(wallet);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created wallet for user: {userId}");
            }

            return new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                PendingBalance = wallet.PendingBalance,
                Currency = wallet.Currency,
                LastWithdrawalAt = wallet.LastWithdrawalAt,
                TotalEarned = wallet.TotalEarned,
                TotalWithdrawn = wallet.TotalWithdrawn,
                TotalTipsReceived = wallet.TotalTipsReceived,
                TotalSubscriptionsEarned = wallet.TotalSubscriptionsEarned,
                TotalPPVEarned = wallet.TotalPPVEarned,
                MinimumWithdrawalAmount = wallet.MinimumWithdrawalAmount,
                IsPayoutAccountVerified = wallet.IsPayoutAccountVerified,
                PayoutAccountVerifiedAt = wallet.PayoutAccountVerifiedAt
            };
        }

        /// <summary>
        /// Actualiza el balance de la billetera
        /// </summary>
        public async Task<bool> UpdateWalletBalanceAsync(Guid userId, decimal amount, TransactionType type)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                _logger.LogWarning($"Wallet not found for user: {userId}");
                return false;
            }

            // Actualizar balance
            wallet.Balance += amount;

            // Actualizar totales según el tipo
            if (amount > 0)
            {
                wallet.TotalEarned += amount;

                switch (type)
                {
                    case TransactionType.Tip:
                        wallet.TotalTipsReceived += amount;
                        break;
                    case TransactionType.Subscription:
                    case TransactionType.SubscriptionRenewal:
                        wallet.TotalSubscriptionsEarned += amount;
                        break;
                    case TransactionType.PostPurchase:
                    case TransactionType.MessagePurchase:
                        wallet.TotalPPVEarned += amount;
                        break;
                }
            }

            wallet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Wallet balance updated for user {userId}: {amount:+#;-#;0} {wallet.Currency}");
            return true;
        }

        /// <summary>
        /// Solicita un retiro de fondos
        /// </summary>
        public async Task<WithdrawalDto> RequestWithdrawalAsync(Guid userId, CreateWithdrawalDto createWithdrawalDto)
        {
            try
            {
                var wallet = await _context.Wallets
                    .Include(w => w.User)
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                    throw new KeyNotFoundException("Wallet not found");

                // Validaciones
                if (wallet.Balance < createWithdrawalDto.Amount)
                    throw new InvalidOperationException("Insufficient balance");

                if (createWithdrawalDto.Amount < wallet.MinimumWithdrawalAmount)
                    throw new InvalidOperationException($"Minimum withdrawal amount is {wallet.MinimumWithdrawalAmount} {wallet.Currency}");

                if (!wallet.IsPayoutAccountVerified)
                    throw new InvalidOperationException("Payout account not verified");

                // Crear solicitud de retiro
                var withdrawal = new Withdrawal
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    Amount = createWithdrawalDto.Amount,
                    Currency = wallet.Currency,
                    Status = WithdrawalStatus.Pending,
                    Method = createWithdrawalDto.Method,
                    Fee = createWithdrawalDto.Amount * WITHDRAWAL_FEE_PERCENTAGE,
                    NetAmount = createWithdrawalDto.Amount - (createWithdrawalDto.Amount * WITHDRAWAL_FEE_PERCENTAGE),
                    AccountDetails = EncryptAccountDetails(createWithdrawalDto.AccountDetails),
                    Notes = createWithdrawalDto.Notes,
                    EstimatedArrivalDays = GetEstimatedArrivalDays(createWithdrawalDto.Method),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Withdrawals.AddAsync(withdrawal);

                // Actualizar balance de la billetera
                wallet.Balance -= createWithdrawalDto.Amount;
                wallet.PendingBalance += createWithdrawalDto.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Notificar al usuario
                await _notificationService.CreateNotificationAsync(
                    userId,
                    NotificationType.WithdrawalProcessed,
                    "Withdrawal Request Received",
                    $"Your withdrawal request for {withdrawal.Amount} {withdrawal.Currency} is being processed.",
                    new { withdrawalId = withdrawal.Id }
                );

                _logger.LogInformation($"Withdrawal requested: {withdrawal.Id} for user {userId}");

                return MapToWithdrawalDto(withdrawal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error requesting withdrawal for user {userId}");
                throw;
            }
        }

        /// <summary>
        /// Procesa un retiro (para administradores)
        /// </summary>
        public async Task<bool> ProcessWithdrawalAsync(Guid withdrawalId, Guid adminUserId)
        {
            try
            {
                var withdrawal = await _context.Withdrawals
                    .Include(w => w.Wallet)
                        .ThenInclude(w => w.User)
                    .FirstOrDefaultAsync(w => w.Id == withdrawalId);

                if (withdrawal == null || withdrawal.Status != WithdrawalStatus.Pending)
                    return false;

                withdrawal.Status = WithdrawalStatus.Processing;
                withdrawal.ProcessedByUserId = adminUserId;
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.UpdatedAt = DateTime.UtcNow;

                // Procesar el pago según el método
                bool payoutSuccess = false;
                string payoutId = null;

                switch (withdrawal.Method)
                {
                    case WithdrawalMethod.Stripe:
                        (payoutSuccess, payoutId) = await ProcessStripePayout(withdrawal);
                        break;
                    case WithdrawalMethod.PayPal:
                        (payoutSuccess, payoutId) = await ProcessPayPalPayout(withdrawal);
                        break;
                    case WithdrawalMethod.BankTransfer:
                        (payoutSuccess, payoutId) = await ProcessBankTransfer(withdrawal);
                        break;
                    default:
                        _logger.LogWarning($"Unsupported withdrawal method: {withdrawal.Method}");
                        break;
                }

                if (payoutSuccess)
                {
                    withdrawal.Status = WithdrawalStatus.Completed;
                    withdrawal.CompletedAt = DateTime.UtcNow;
                    withdrawal.TransactionReference = payoutId;

                    // Actualizar billetera
                    withdrawal.Wallet.PendingBalance -= withdrawal.Amount;
                    withdrawal.Wallet.TotalWithdrawn += withdrawal.Amount;
                    withdrawal.Wallet.LastWithdrawalAt = DateTime.UtcNow;

                    // Crear transacción
                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = withdrawal.Wallet.UserId,
                        Type = TransactionType.Withdrawal,
                        Status = TransactionStatus.Completed,
                        Amount = -withdrawal.Amount,
                        Currency = withdrawal.Currency,
                        Fee = withdrawal.Fee,
                        NetAmount = -withdrawal.NetAmount,
                        Description = $"Withdrawal via {withdrawal.Method}",
                        ExternalTransactionId = payoutId,
                        WithdrawalId = withdrawal.Id,
                        ProcessedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.Transactions.AddAsync(transaction);

                    // Notificar al usuario
                    await _emailService.SendWithdrawalProcessedAsync(
                        withdrawal.Wallet.User.Email,
                        withdrawal.NetAmount,
                        withdrawal.Method.ToString()
                    );

                    await _notificationService.CreateNotificationAsync(
                        withdrawal.Wallet.UserId,
                        NotificationType.WithdrawalProcessed,
                        "Withdrawal Completed",
                        $"Your withdrawal of {withdrawal.NetAmount} {withdrawal.Currency} has been sent.",
                        new { withdrawalId = withdrawal.Id, payoutId }
                    );
                }
                else
                {
                    withdrawal.Status = WithdrawalStatus.Failed;

                    // Devolver fondos a la billetera
                    withdrawal.Wallet.Balance += withdrawal.Amount;
                    withdrawal.Wallet.PendingBalance -= withdrawal.Amount;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Withdrawal processed: {withdrawalId} - Status: {withdrawal.Status}");
                return payoutSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing withdrawal: {withdrawalId}");
                return false;
            }
        }

        /// <summary>
        /// Rechaza un retiro (para administradores)
        /// </summary>
        public async Task<bool> RejectWithdrawalAsync(Guid withdrawalId, Guid adminUserId, string reason)
        {
            var withdrawal = await _context.Withdrawals
                .Include(w => w.Wallet)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null || withdrawal.Status != WithdrawalStatus.Pending)
                return false;

            withdrawal.Status = WithdrawalStatus.Rejected;
            withdrawal.RejectionReason = reason;
            withdrawal.RejectedAt = DateTime.UtcNow;
            withdrawal.ProcessedByUserId = adminUserId;
            withdrawal.UpdatedAt = DateTime.UtcNow;

            // Devolver fondos a la billetera
            withdrawal.Wallet.Balance += withdrawal.Amount;
            withdrawal.Wallet.PendingBalance -= withdrawal.Amount;
            withdrawal.Wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notificar al usuario
            await _notificationService.CreateNotificationAsync(
                withdrawal.Wallet.UserId,
                NotificationType.SystemAlert,
                "Withdrawal Rejected",
                $"Your withdrawal request has been rejected. Reason: {reason}",
                new { withdrawalId = withdrawal.Id }
            );

            _logger.LogInformation($"Withdrawal rejected: {withdrawalId} by admin {adminUserId}");
            return true;
        }

        /// <summary>
        /// Obtiene los retiros de un usuario
        /// </summary>
        public async Task<PaginatedResultDto<WithdrawalDto>> GetUserWithdrawalsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                return new PaginatedResultDto<WithdrawalDto>(new List<WithdrawalDto>(), 0, page, pageSize);

            var query = _context.Withdrawals
                .Where(w => w.WalletId == wallet.Id)
                .OrderByDescending(w => w.CreatedAt);

            var totalCount = await query.CountAsync();

            var withdrawals = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => MapToWithdrawalDto(w))
                .ToListAsync();

            return new PaginatedResultDto<WithdrawalDto>(withdrawals, totalCount, page, pageSize);
        }

        /// <summary>
        /// Envía una propina
        /// </summary>
        public async Task<TipDto> SendTipAsync(Guid senderId, CreateTipDto createTipDto)
        {
            try
            {
                // Validar que no se envíe propina a sí mismo
                if (senderId == createTipDto.RecipientId)
                    throw new InvalidOperationException("Cannot send tip to yourself");

                // Crear la propina
                var tip = new Tip
                {
                    Id = Guid.NewGuid(),
                    SenderId = senderId,
                    RecipientId = createTipDto.RecipientId,
                    Amount = createTipDto.Amount,
                    Currency = "USD",
                    Message = createTipDto.Message,
                    PostId = createTipDto.PostId,
                    IsAnonymous = createTipDto.IsAnonymous,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Tips.AddAsync(tip);

                // Procesar el pago
                var paymentDto = new CreatePaymentDto
                {
                    Type = TransactionType.Tip,
                    Amount = tip.Amount,
                    Currency = tip.Currency,
                    PaymentMethodId = createTipDto.PaymentMethodId,
                    TipRecipientId = createTipDto.RecipientId,
                    Description = $"Tip to creator"
                };

                var payment = await ProcessPaymentAsync(senderId, paymentDto);

                if (payment.Status == TransactionStatus.Completed)
                {
                    tip.RecipientNotified = true;

                    // Actualizar billetera del receptor (después de comisión)
                    var netAmount = tip.Amount * (1 - PLATFORM_FEE_PERCENTAGE);
                    await UpdateWalletBalanceAsync(createTipDto.RecipientId, netAmount, TransactionType.Tip);

                    // Notificar al receptor
                    if (!tip.IsAnonymous)
                    {
                        var sender = await _context.Users.FindAsync(senderId);
                        await _notificationService.CreateNotificationAsync(
                            createTipDto.RecipientId,
                            NotificationType.NewTip,
                            "New Tip Received!",
                            $"{sender?.DisplayName ?? "Someone"} sent you a ${tip.Amount} tip!",
                            new { tipId = tip.Id, senderId, amount = tip.Amount }
                        );
                    }
                    else
                    {
                        await _notificationService.CreateNotificationAsync(
                            createTipDto.RecipientId,
                            NotificationType.NewTip,
                            "New Anonymous Tip!",
                            $"You received a ${tip.Amount} tip!",
                            new { tipId = tip.Id, amount = tip.Amount }
                        );
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Tip sent: {tip.Id} from {senderId} to {createTipDto.RecipientId}");
                }
                else
                {
                    // Si el pago falla, eliminar la propina
                    _context.Tips.Remove(tip);
                    await _context.SaveChangesAsync();
                    throw new InvalidOperationException("Payment failed");
                }

                return MapToTipDto(tip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending tip from {senderId}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene las propinas recibidas por un usuario
        /// </summary>
        public async Task<PaginatedResultDto<TipDto>> GetUserTipsReceivedAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Tips
                .Include(t => t.Sender)
                .Include(t => t.Post)
                .Where(t => t.RecipientId == userId)
                .OrderByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();

            var tips = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToTipDto(t))
                .ToListAsync();

            return new PaginatedResultDto<TipDto>(tips, totalCount, page, pageSize);
        }

        /// <summary>
        /// Obtiene las propinas enviadas por un usuario
        /// </summary>
        public async Task<PaginatedResultDto<TipDto>> GetUserTipsSentAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Tips
                .Include(t => t.Recipient)
                .Include(t => t.Post)
                .Where(t => t.SenderId == userId)
                .OrderByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();

            var tips = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToTipDto(t))
                .ToListAsync();

            return new PaginatedResultDto<TipDto>(tips, totalCount, page, pageSize);
        }

        /// <summary>
        /// Configura la cuenta de pago para retiros
        /// </summary>
        public async Task<bool> SetupPayoutAccountAsync(Guid userId, string accountType, string accountDetails)
        {
            try
            {
                var wallet = await _context.Wallets
                    .Include(w => w.User)
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                    return false;

                // Crear cuenta conectada en Stripe
                if (accountType.ToLower() == "stripe")
                {
                    var accountOptions = new AccountCreateOptions
                    {
                        Type = "express",
                        Country = "US", // Ajustar según el país del usuario
                        Email = wallet.User.Email,
                        Capabilities = new AccountCapabilitiesOptions
                        {
                            CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                            Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
                        },
                        BusinessProfile = new AccountBusinessProfileOptions
                        {
                            Mcc = "5734", // Computer Software Stores
                            Name = wallet.User.DisplayName,
                            Url = $"{_configuration["App:BaseUrl"]}/creator/{wallet.User.Username}"
                        },
                        Metadata = new Dictionary<string, string>
                        {
                            { "user_id", userId.ToString() },
                            { "wallet_id", wallet.Id.ToString() }
                        }
                    };

                    var account = await _stripeAccountService.CreateAsync(accountOptions);
                    wallet.StripeAccountId = account.Id;

                    // Generar link de onboarding
                    var accountLinkService = new AccountLinkService();
                    var linkOptions = new AccountLinkCreateOptions
                    {
                        Account = account.Id,
                        RefreshUrl = $"{_configuration["App:BaseUrl"]}/settings/payouts",
                        ReturnUrl = $"{_configuration["App:BaseUrl"]}/settings/payouts/success",
                        Type = "account_onboarding"
                    };

                    var accountLink = await accountLinkService.CreateAsync(linkOptions);

                    // Guardar el link para enviarlo al usuario
                    // En producción, enviarías este link al frontend
                    _logger.LogInformation($"Stripe onboarding link created for user {userId}: {accountLink.Url}");
                }
                else if (accountType.ToLower() == "paypal")
                {
                    wallet.PayPalEmail = accountDetails;
                }
                else if (accountType.ToLower() == "bank")
                {
                    wallet.BankAccountInfo = EncryptAccountDetails(accountDetails);
                }

                wallet.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payout account configured for user {userId}: {accountType}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting up payout account for user {userId}");
                return false;
            }
        }

        /// <summary>
        /// Verifica la cuenta de pago
        /// </summary>
        public async Task<bool> VerifyPayoutAccountAsync(Guid userId)
        {
            try
            {
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                    return false;

                // Verificar cuenta de Stripe si existe
                if (!string.IsNullOrEmpty(wallet.StripeAccountId))
                {
                    var account = await _stripeAccountService.GetAsync(wallet.StripeAccountId);

                    if (account.ChargesEnabled && account.PayoutsEnabled)
                    {
                        wallet.IsPayoutAccountVerified = true;
                        wallet.PayoutAccountVerifiedAt = DateTime.UtcNow;
                        wallet.UpdatedAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();

                        await _notificationService.CreateNotificationAsync(
                            userId,
                            NotificationType.VerificationCompleted,
                            "Payout Account Verified",
                            "Your payout account has been verified. You can now request withdrawals.",
                            null
                        );

                        _logger.LogInformation($"Payout account verified for user {userId}");
                        return true;
                    }
                }
                else if (!string.IsNullOrEmpty(wallet.PayPalEmail) || !string.IsNullOrEmpty(wallet.BankAccountInfo))
                {
                    // Para PayPal y banco, verificación manual por admin
                    wallet.IsPayoutAccountVerified = true;
                    wallet.PayoutAccountVerifiedAt = DateTime.UtcNow;
                    wallet.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying payout account for user {userId}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el desglose de ganancias
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetEarningsBreakdownAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId &&
                           t.CreatedAt >= startDate &&
                           t.CreatedAt <= endDate &&
                           t.Status == TransactionStatus.Completed &&
                           t.NetAmount > 0)
                .ToListAsync();

            var breakdown = new Dictionary<string, decimal>
            {
                ["Subscriptions"] = transactions
                    .Where(t => t.Type == TransactionType.Subscription || t.Type == TransactionType.SubscriptionRenewal)
                    .Sum(t => t.NetAmount),
                ["Tips"] = transactions
                    .Where(t => t.Type == TransactionType.Tip)
                    .Sum(t => t.NetAmount),
                ["PPV Content"] = transactions
                    .Where(t => t.Type == TransactionType.PostPurchase)
                    .Sum(t => t.NetAmount),
                ["Messages"] = transactions
                    .Where(t => t.Type == TransactionType.MessagePurchase)
                    .Sum(t => t.NetAmount),
                ["Total Earnings"] = transactions.Sum(t => t.NetAmount),
                ["Platform Fees"] = transactions.Sum(t => t.Fee ?? 0),
                ["Gross Revenue"] = transactions.Sum(t => t.Amount)
            };

            // Calcular estadísticas adicionales
            breakdown["Average Transaction"] = transactions.Any() ?
                breakdown["Total Earnings"] / transactions.Count : 0;
            breakdown["Transaction Count"] = transactions.Count;

            return breakdown;
        }

        /// <summary>
        /// Procesa las comisiones de la plataforma (job programado)
        /// </summary>
        public async Task<bool> ProcessPlatformFeesAsync()
        {
            try
            {
                // Obtener transacciones pendientes de procesar fees
                var pendingTransactions = await _context.Transactions
                    .Where(t => t.Status == TransactionStatus.Completed &&
                               t.Fee == null &&
                               t.Type != TransactionType.Withdrawal &&
                               t.Type != TransactionType.Refund)
                    .ToListAsync();

                foreach (var transaction in pendingTransactions)
                {
                    transaction.Fee = transaction.Amount * PLATFORM_FEE_PERCENTAGE;
                    transaction.NetAmount = transaction.Amount - transaction.Fee.Value;
                    transaction.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Processed platform fees for {pendingTransactions.Count} transactions");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing platform fees");
                return false;
            }
        }

        #region Métodos Privados de Ayuda

        /// <summary>
        /// Obtiene o crea un cliente de Stripe
        /// </summary>
        private async Task<string> GetOrCreateStripeCustomer(User user)
        {
            // Buscar si ya existe un cliente
            var customers = await _stripeCustomerService.ListAsync(new CustomerListOptions
            {
                Email = user.Email,
                Limit = 1
            });

            if (customers.Data.Any())
            {
                return customers.Data.First().Id;
            }

            // Crear nuevo cliente
            var customerOptions = new CustomerCreateOptions
            {
                Email = user.Email,
                Name = user.DisplayName,
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", user.Id.ToString() },
                    { "username", user.Username }
                }
            };

            var customer = await _stripeCustomerService.CreateAsync(customerOptions);
            return customer.Id;
        }

        /// <summary>
        /// Procesa un pago exitoso
        /// </summary>
        private async Task ProcessSuccessfulPayment(Transaction transaction, CreatePaymentDto paymentDto)
        {
            switch (transaction.Type)
            {
                case TransactionType.Subscription:
                case TransactionType.SubscriptionRenewal:
                    if (paymentDto.SubscriptionId.HasValue)
                    {
                        var subscription = await _context.Subscriptions.FindAsync(paymentDto.SubscriptionId.Value);
                        if (subscription != null)
                        {
                            subscription.IsActive = true;
                            subscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);

                            // Actualizar billetera del creador
                            await UpdateWalletBalanceAsync(
                                subscription.CreatorId,
                                transaction.NetAmount,
                                transaction.Type);
                        }
                    }
                    break;

                case TransactionType.PostPurchase:
                    if (paymentDto.PostId.HasValue)
                    {
                        var post = await _context.Posts.FindAsync(paymentDto.PostId.Value);
                        if (post != null)
                        {
                            var purchase = new PostPurchase
                            {
                                Id = Guid.NewGuid(),
                                UserId = transaction.UserId,
                                PostId = post.Id,
                                PricePaid = transaction.Amount,
                                Currency = transaction.Currency,
                                TransactionId = transaction.Id,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            await _context.PostPurchases.AddAsync(purchase);

                            // Actualizar billetera del creador
                            await UpdateWalletBalanceAsync(
                                post.CreatorId,
                                transaction.NetAmount,
                                transaction.Type);
                        }
                    }
                    break;

                case TransactionType.MessagePurchase:
                    if (paymentDto.MessageId.HasValue)
                    {
                        var message = await _context.Messages.FindAsync(paymentDto.MessageId.Value);
                        if (message != null)
                        {
                            message.IsPaid = true;
                            message.PaidAt = DateTime.UtcNow;

                            // Actualizar billetera del emisor
                            await UpdateWalletBalanceAsync(
                                message.SenderId,
                                transaction.NetAmount,
                                transaction.Type);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Revierte los efectos de un pago (para reembolsos)
        /// </summary>
        private async Task RevertPaymentEffects(Transaction transaction)
        {
            switch (transaction.Type)
            {
                case TransactionType.Subscription:
                case TransactionType.SubscriptionRenewal:
                    if (transaction.SubscriptionId.HasValue)
                    {
                        var subscription = await _context.Subscriptions.FindAsync(transaction.SubscriptionId.Value);
                        if (subscription != null)
                        {
                            subscription.IsActive = false;
                            subscription.EndDate = DateTime.UtcNow;

                            // Revertir balance del creador
                            await UpdateWalletBalanceAsync(
                                subscription.CreatorId,
                                -transaction.NetAmount,
                                TransactionType.Refund);
                        }
                    }
                    break;

                case TransactionType.PostPurchase:
                    if (transaction.PostId.HasValue)
                    {
                        var purchase = await _context.PostPurchases
                            .FirstOrDefaultAsync(pp => pp.TransactionId == transaction.Id);

                        if (purchase != null)
                        {
                            _context.PostPurchases.Remove(purchase);

                            var post = await _context.Posts.FindAsync(transaction.PostId.Value);
                            if (post != null)
                            {
                                await UpdateWalletBalanceAsync(
                                    post.CreatorId,
                                    -transaction.NetAmount,
                                    TransactionType.Refund);
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Procesa un pago con Stripe
        /// </summary>
        private async Task<(bool success, string payoutId)> ProcessStripePayout(Withdrawal withdrawal)
        {
            try
            {
                if (string.IsNullOrEmpty(withdrawal.Wallet.StripeAccountId))
                {
                    _logger.LogWarning($"No Stripe account for withdrawal {withdrawal.Id}");
                    return (false, null);
                }

                // Crear transferencia a la cuenta conectada
                var transferOptions = new TransferCreateOptions
                {
                    Amount = ConvertToStripAmount(withdrawal.NetAmount, withdrawal.Currency),
                    Currency = withdrawal.Currency.ToLower(),
                    Destination = withdrawal.Wallet.StripeAccountId,
                    Description = $"Withdrawal {withdrawal.Id}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "withdrawal_id", withdrawal.Id.ToString() },
                        { "user_id", withdrawal.Wallet.UserId.ToString() }
                    }
                };

                var transfer = await _stripeTransferService.CreateAsync(transferOptions);

                // Crear payout en la cuenta conectada
                var payoutOptions = new PayoutCreateOptions
                {
                    Amount = ConvertToStripAmount(withdrawal.NetAmount, withdrawal.Currency),
                    Currency = withdrawal.Currency.ToLower(),
                    Method = "standard",
                    Description = $"Arsmate earnings withdrawal",
                    Metadata = new Dictionary<string, string>
                    {
                        { "withdrawal_id", withdrawal.Id.ToString() }
                    }
                };

                var requestOptions = new RequestOptions
                {
                    StripeAccount = withdrawal.Wallet.StripeAccountId
                };

                var payout = await _stripePayoutService.CreateAsync(payoutOptions, requestOptions);

                return (true, payout.Id);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"Stripe error processing payout for withdrawal {withdrawal.Id}");
                return (false, null);
            }
        }

        /// <summary>
        /// Procesa un pago con PayPal
        /// </summary>
        private async Task<(bool success, string payoutId)> ProcessPayPalPayout(Withdrawal withdrawal)
        {
            // TODO: Implementar integración con PayPal Payouts API
            _logger.LogInformation($"Processing PayPal payout for withdrawal {withdrawal.Id}");

            // Simulación para desarrollo
            await Task.Delay(1000);
            return (true, $"PAYPAL-{Guid.NewGuid()}");
        }

        /// <summary>
        /// Procesa una transferencia bancaria
        /// </summary>
        private async Task<(bool success, string transferId)> ProcessBankTransfer(Withdrawal withdrawal)
        {
            // TODO: Implementar integración con servicio de transferencias bancarias
            _logger.LogInformation($"Processing bank transfer for withdrawal {withdrawal.Id}");

            // Simulación para desarrollo
            await Task.Delay(1000);
            return (true, $"BANK-{Guid.NewGuid()}");
        }

        /// <summary>
        /// Convierte monto a centavos para Stripe
        /// </summary>
        private long ConvertToStripAmount(decimal amount, string currency)
        {
            // Stripe usa la unidad más pequeña (centavos para USD)
            return (long)(amount * 100);
        }

        /// <summary>
        /// Encripta los detalles de la cuenta
        /// </summary>
        private string EncryptAccountDetails(string details)
        {
            // TODO: Implementar encriptación real
            // Por ahora, solo retornamos el string
            return details;
        }

        /// <summary>
        /// Obtiene los días estimados de llegada según el método
        /// </summary>
        private int GetEstimatedArrivalDays(WithdrawalMethod method)
        {
            return method switch
            {
                WithdrawalMethod.PayPal => 1,
                WithdrawalMethod.Stripe => 2,
                WithdrawalMethod.BankTransfer => 3,
                WithdrawalMethod.WireTransfer => 5,
                _ => 7
            };
        }

        /// <summary>
        /// Obtiene la IP del cliente
        /// </summary>
        private string GetClientIpAddress()
        {
            // TODO: Obtener desde HttpContext
            return "127.0.0.1";
        }

        /// <summary>
        /// Obtiene el código de país desde la IP
        /// </summary>
        private async Task<string> GetCountryCodeFromIp()
        {
            // TODO: Implementar con servicio de geolocalización
            return await Task.FromResult("US");
        }

        /// <summary>
        /// Mapea Transaction a PaymentDto
        /// </summary>
        private PaymentDto MapToPaymentDto(Transaction transaction)
        {
            return new PaymentDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Type = transaction.Type,
                Status = transaction.Status,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Fee = transaction.Fee,
                NetAmount = transaction.NetAmount,
                Description = transaction.Description,
                PaymentMethod = transaction.PaymentMethod,
                CardLast4 = transaction.CardLast4,
                CreatedAt = transaction.CreatedAt,
                ProcessedAt = transaction.ProcessedAt
            };
        }

        /// <summary>
        /// Mapea Withdrawal a WithdrawalDto
        /// </summary>
        private WithdrawalDto MapToWithdrawalDto(Withdrawal withdrawal)
        {
            return new WithdrawalDto
            {
                Id = withdrawal.Id,
                WalletId = withdrawal.WalletId,
                Amount = withdrawal.Amount,
                Currency = withdrawal.Currency,
                Status = withdrawal.Status,
                Method = withdrawal.Method,
                Fee = withdrawal.Fee,
                NetAmount = withdrawal.NetAmount,
                CreatedAt = withdrawal.CreatedAt,
                ProcessedAt = withdrawal.ProcessedAt,
                CompletedAt = withdrawal.CompletedAt,
                RejectionReason = withdrawal.RejectionReason,
                EstimatedArrivalDays = withdrawal.EstimatedArrivalDays
            };
        }

        /// <summary>
        /// Mapea Tip a TipDto
        /// </summary>
        private TipDto MapToTipDto(Tip tip)
        {
            return new TipDto
            {
                Id = tip.Id,
                SenderId = tip.SenderId,
                RecipientId = tip.RecipientId,
                Amount = tip.Amount,
                Currency = tip.Currency,
                Message = tip.Message,
                PostId = tip.PostId,
                IsAnonymous = tip.IsAnonymous,
                CreatedAt = tip.CreatedAt
            };
        }

        #endregion
    }
}