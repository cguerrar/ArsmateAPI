using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Arsmate.Core.DTOs.Payment;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Enums;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de pagos
    /// </summary>
    public interface IPaymentService
    {
        Task<PaymentDto> ProcessPaymentAsync(Guid userId, CreatePaymentDto createPaymentDto);
        Task<PaymentDto> GetPaymentByIdAsync(Guid paymentId);
        Task<PaginatedResultDto<PaymentDto>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<bool> RefundPaymentAsync(Guid paymentId, string reason);
        Task<WalletDto> GetWalletAsync(Guid userId);
        Task<bool> UpdateWalletBalanceAsync(Guid userId, decimal amount, TransactionType type);
        Task<WithdrawalDto> RequestWithdrawalAsync(Guid userId, CreateWithdrawalDto createWithdrawalDto);
        Task<bool> ProcessWithdrawalAsync(Guid withdrawalId, Guid adminUserId);
        Task<bool> RejectWithdrawalAsync(Guid withdrawalId, Guid adminUserId, string reason);
        Task<PaginatedResultDto<WithdrawalDto>> GetUserWithdrawalsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<TipDto> SendTipAsync(Guid senderId, CreateTipDto createTipDto);
        Task<PaginatedResultDto<TipDto>> GetUserTipsReceivedAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<TipDto>> GetUserTipsSentAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<bool> SetupPayoutAccountAsync(Guid userId, string accountType, string accountDetails);
        Task<bool> VerifyPayoutAccountAsync(Guid userId);
        Task<Dictionary<string, decimal>> GetEarningsBreakdownAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<bool> ProcessPlatformFeesAsync();
    }
}
