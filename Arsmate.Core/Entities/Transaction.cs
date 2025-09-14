// ========================================
// Archivo: Arsmate.Core/Entities/Transaction.cs
// ========================================

using System;
using System.ComponentModel.DataAnnotations;
// Especificar explícitamente el namespace para evitar ambigüedad
using Arsmate.Core.Enums;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa una transacción financiera en el sistema
    /// </summary>
    public class Transaction : BaseEntity
    {
        /// <summary>
        /// ID del usuario involucrado en la transacción
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tipo de transacción
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Estado de la transacción - Usando el namespace completo para evitar ambigüedad
        /// </summary>
        public Arsmate.Core.Enums.TransactionStatus Status { get; set; }

        /// <summary>
        /// Monto de la transacción
        /// </summary>
        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe estar entre 0.01 y 999999.99")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Moneda de la transacción
        /// </summary>
        [Required]
        [StringLength(3, ErrorMessage = "El código de moneda debe ser de 3 caracteres")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Comisión cobrada por la plataforma
        /// </summary>
        [Range(0, 999999.99, ErrorMessage = "La comisión debe estar entre 0 y 999999.99")]
        public decimal? Fee { get; set; }

        /// <summary>
        /// Monto neto después de comisiones
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Descripción de la transacción
        /// </summary>
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Description { get; set; }

        /// <summary>
        /// ID de transacción externo (Stripe, PayPal, etc.)
        /// </summary>
        [StringLength(255, ErrorMessage = "El ID externo no puede exceder 255 caracteres")]
        public string ExternalTransactionId { get; set; }

        /// <summary>
        /// Método de pago utilizado
        /// </summary>
        [StringLength(50, ErrorMessage = "El método de pago no puede exceder 50 caracteres")]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Últimos 4 dígitos de la tarjeta (si aplica)
        /// </summary>
        [StringLength(4, ErrorMessage = "Los últimos dígitos deben ser 4 caracteres")]
        public string CardLast4 { get; set; }

        /// <summary>
        /// Dirección IP desde donde se realizó la transacción
        /// </summary>
        [StringLength(45, ErrorMessage = "La IP no puede exceder 45 caracteres")]
        public string IpAddress { get; set; }

        /// <summary>
        /// País desde donde se realizó la transacción
        /// </summary>
        [StringLength(2, ErrorMessage = "El código de país debe ser de 2 caracteres")]
        public string CountryCode { get; set; }

        // Referencias opcionales a entidades relacionadas
        public Guid? SubscriptionId { get; set; }
        public Guid? PostId { get; set; }
        public Guid? MessageId { get; set; }
        public Guid? TipId { get; set; }
        public Guid? WithdrawalId { get; set; }

        /// <summary>
        /// Fecha de procesamiento de la transacción
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Fecha de fallo (si aplica)
        /// </summary>
        public DateTime? FailedAt { get; set; }

        /// <summary>
        /// Razón del fallo (si aplica)
        /// </summary>
        [StringLength(500, ErrorMessage = "La razón del fallo no puede exceder 500 caracteres")]
        public string FailureReason { get; set; }

        // Relaciones
        public virtual User User { get; set; }
        public virtual Subscription Subscription { get; set; }
        public virtual Post Post { get; set; }
        public virtual Message Message { get; set; }
        public virtual Tip Tip { get; set; }
        public virtual Withdrawal Withdrawal { get; set; }
    }
}