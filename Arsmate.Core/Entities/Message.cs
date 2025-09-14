using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa un mensaje directo entre usuarios
    /// </summary>
    public class Message : BaseEntity
    {
        /// <summary>
        /// ID del usuario que envía el mensaje
        /// </summary>
        [Required]
        public Guid SenderId { get; set; }

        /// <summary>
        /// ID del usuario que recibe el mensaje
        /// </summary>
        [Required]
        public Guid RecipientId { get; set; }

        /// <summary>
        /// Contenido del mensaje
        /// </summary>
        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
        public string Content { get; set; }

        /// <summary>
        /// Indica si el mensaje ha sido leído
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Fecha y hora de lectura
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Precio del mensaje (para mensajes pagos)
        /// </summary>
        [Range(0, 9999.99, ErrorMessage = "El precio debe estar entre 0 y 9999.99")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Indica si el mensaje ha sido pagado
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Fecha de pago del mensaje
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Indica si el mensaje ha sido eliminado por el emisor
        /// </summary>
        public bool DeletedBySender { get; set; }

        /// <summary>
        /// Indica si el mensaje ha sido eliminado por el receptor
        /// </summary>
        public bool DeletedByRecipient { get; set; }

        /// <summary>
        /// ID del mensaje al que responde (para threads)
        /// </summary>
        public Guid? ReplyToMessageId { get; set; }

        /// <summary>
        /// Indica si es un mensaje de propina
        /// </summary>
        public bool IsTipMessage { get; set; }

        /// <summary>
        /// Monto de la propina asociada
        /// </summary>
        [Range(0, 9999.99, ErrorMessage = "La propina debe estar entre 0 y 9999.99")]
        public decimal? TipAmount { get; set; }

        // Relaciones
        public virtual User Sender { get; set; }
        public virtual User Recipient { get; set; }
        public virtual Message ReplyToMessage { get; set; }
        public virtual ICollection<MediaFile> Attachments { get; set; } = new List<MediaFile>();
    }
}