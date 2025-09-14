using System;
using System.ComponentModel.DataAnnotations;
using Arsmate.Core.Enums;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa una solicitud de retiro de fondos
    /// </summary>
    public class Withdrawal : BaseEntity
    {
        /// <summary>
        /// ID de la billetera desde donde se retira
        /// </summary>
        [Required]
        public Guid WalletId { get; set; }

        /// <summary>
        /// Monto a retirar
        /// </summary>
        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe estar entre 0.01 y 999999.99")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Moneda del retiro
        /// </summary>
        [StringLength(3, ErrorMessage = "El código de moneda debe ser de 3 caracteres")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Estado del retiro
        /// </summary>
        public WithdrawalStatus Status { get; set; }

        /// <summary>
        /// Método de retiro
        /// </summary>
        public WithdrawalMethod Method { get; set; }

        /// <summary>
        /// Referencia de la transacción externa
        /// </summary>
        [StringLength(255, ErrorMessage = "La referencia no puede exceder 255 caracteres")]
        public string TransactionReference { get; set; }

        /// <summary>
        /// Fecha de procesamiento
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Fecha de completado
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Fecha de rechazo
        /// </summary>
        public DateTime? RejectedAt { get; set; }

        /// <summary>
        /// Razón del rechazo
        /// </summary>
        [StringLength(500, ErrorMessage = "La razón del rechazo no puede exceder 500 caracteres")]
        public string RejectionReason { get; set; }

        /// <summary>
        /// Comisión cobrada por el retiro
        /// </summary>
        [Range(0, 999.99, ErrorMessage = "La comisión debe estar entre 0 y 999.99")]
        public decimal Fee { get; set; }

        /// <summary>
        /// Monto neto a recibir
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Detalles de la cuenta destino (encriptado)
        /// </summary>
        public string AccountDetails { get; set; }

        /// <summary>
        /// Notas adicionales
        /// </summary>
        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string Notes { get; set; }

        /// <summary>
        /// ID del administrador que procesó el retiro
        /// </summary>
        public Guid? ProcessedByUserId { get; set; }

        /// <summary>
        /// Tiempo estimado de llegada en días
        /// </summary>
        public int? EstimatedArrivalDays { get; set; }

        // Relaciones
        public virtual Wallet Wallet { get; set; }
        public virtual User ProcessedByUser { get; set; }
    }
}