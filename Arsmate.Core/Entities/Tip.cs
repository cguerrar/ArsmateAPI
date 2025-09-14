using System;
using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa una propina enviada entre usuarios
    /// </summary>
    public class Tip : BaseEntity
    {
        /// <summary>
        /// ID del usuario que envía la propina
        /// </summary>
        [Required]
        public Guid SenderId { get; set; }

        /// <summary>
        /// ID del usuario que recibe la propina
        /// </summary>
        [Required]
        public Guid RecipientId { get; set; }

        /// <summary>
        /// Monto de la propina
        /// </summary>
        [Required]
        [Range(0.01, 9999.99, ErrorMessage = "La propina debe estar entre 0.01 y 9999.99")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Moneda de la propina
        /// </summary>
        [StringLength(3, ErrorMessage = "El código de moneda debe ser de 3 caracteres")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Mensaje asociado a la propina
        /// </summary>
        [StringLength(200, ErrorMessage = "El mensaje no puede exceder 200 caracteres")]
        public string Message { get; set; }

        /// <summary>
        /// ID de la publicación asociada (si aplica)
        /// </summary>
        public Guid? PostId { get; set; }

        /// <summary>
        /// ID del mensaje asociado (si aplica)
        /// </summary>
        public Guid? MessageId { get; set; }

        /// <summary>
        /// ID del stream en vivo asociado (si aplica)
        /// </summary>
        public Guid? LiveStreamId { get; set; }

        /// <summary>
        /// Indica si la propina es anónima
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Indica si el receptor ha sido notificado
        /// </summary>
        public bool RecipientNotified { get; set; }

        // Relaciones
        public virtual User Sender { get; set; }
        public virtual User Recipient { get; set; }
        public virtual Post Post { get; set; }
        public virtual Message AssociatedMessage { get; set; }
    }
}