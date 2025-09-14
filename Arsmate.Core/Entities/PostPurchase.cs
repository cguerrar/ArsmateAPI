using System;
using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa la compra de contenido Pay-Per-View
    /// </summary>
    public class PostPurchase : BaseEntity
    {
        /// <summary>
        /// ID del usuario que compra
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// ID de la publicación comprada
        /// </summary>
        [Required]
        public Guid PostId { get; set; }

        /// <summary>
        /// Precio pagado
        /// </summary>
        [Required]
        [Range(0.01, 9999.99, ErrorMessage = "El precio debe estar entre 0.01 y 9999.99")]
        public decimal PricePaid { get; set; }

        /// <summary>
        /// Moneda del pago
        /// </summary>
        [StringLength(3, ErrorMessage = "El código de moneda debe ser de 3 caracteres")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// ID de la transacción asociada
        /// </summary>
        public Guid? TransactionId { get; set; }

        /// <summary>
        /// Fecha de expiración del acceso (si aplica)
        /// </summary>
        public DateTime? AccessExpiresAt { get; set; }

        /// <summary>
        /// Número de veces que ha visto el contenido
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// Última vez que vio el contenido
        /// </summary>
        public DateTime? LastViewedAt { get; set; }

        // Relaciones
        public virtual User User { get; set; }
        public virtual Post Post { get; set; }
        public virtual Transaction Transaction { get; set; }
    }
}