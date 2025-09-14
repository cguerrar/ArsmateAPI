
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa la billetera digital de un usuario
    /// </summary>
    public class Wallet : BaseEntity
    {
        /// <summary>
        /// ID del usuario propietario de la billetera
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Balance disponible para retiro
        /// </summary>
        [Range(0, 9999999.99, ErrorMessage = "El balance debe estar entre 0 y 9999999.99")]
        public decimal Balance { get; set; }

        /// <summary>
        /// Balance pendiente (no disponible aún)
        /// </summary>
        [Range(0, 9999999.99, ErrorMessage = "El balance pendiente debe estar entre 0 y 9999999.99")]
        public decimal PendingBalance { get; set; }

        /// <summary>
        /// Moneda de la billetera
        /// </summary>
        [StringLength(3, ErrorMessage = "El código de moneda debe ser de 3 caracteres")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Fecha del último retiro
        /// </summary>
        public DateTime? LastWithdrawalAt { get; set; }

        /// <summary>
        /// Total ganado históricamente
        /// </summary>
        [Range(0, 999999999.99, ErrorMessage = "El total ganado debe estar entre 0 y 999999999.99")]
        public decimal TotalEarned { get; set; }

        /// <summary>
        /// Total retirado históricamente
        /// </summary>
        [Range(0, 999999999.99, ErrorMessage = "El total retirado debe estar entre 0 y 999999999.99")]
        public decimal TotalWithdrawn { get; set; }

        /// <summary>
        /// Total de propinas recibidas
        /// </summary>
        [Range(0, 999999999.99, ErrorMessage = "El total de propinas debe estar entre 0 y 999999999.99")]
        public decimal TotalTipsReceived { get; set; }

        /// <summary>
        /// Total de suscripciones ganadas
        /// </summary>
        [Range(0, 999999999.99, ErrorMessage = "El total de suscripciones debe estar entre 0 y 999999999.99")]
        public decimal TotalSubscriptionsEarned { get; set; }

        /// <summary>
        /// Total de PPV ganado
        /// </summary>
        [Range(0, 999999999.99, ErrorMessage = "El total PPV debe estar entre 0 y 999999999.99")]
        public decimal TotalPPVEarned { get; set; }

        /// <summary>
        /// Monto mínimo para retiro
        /// </summary>
        [Range(0, 1000, ErrorMessage = "El mínimo de retiro debe estar entre 0 y 1000")]
        public decimal MinimumWithdrawalAmount { get; set; } = 20;

        /// <summary>
        /// Información de cuenta bancaria (encriptada)
        /// </summary>
        public string BankAccountInfo { get; set; }

        /// <summary>
        /// Email de PayPal
        /// </summary>
        [EmailAddress(ErrorMessage = "El email de PayPal no es válido")]
        public string PayPalEmail { get; set; }

        /// <summary>
        /// ID de cuenta de Stripe Connect
        /// </summary>
        public string StripeAccountId { get; set; }

        /// <summary>
        /// Indica si la cuenta de pago está verificada
        /// </summary>
        public bool IsPayoutAccountVerified { get; set; }

        /// <summary>
        /// Fecha de verificación de la cuenta
        /// </summary>
        public DateTime? PayoutAccountVerifiedAt { get; set; }

        // Relaciones
        public virtual User User { get; set; }
        public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
    }
}