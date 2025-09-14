using System;
using System.ComponentModel.DataAnnotations;
using Arsmate.Core.Enums;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa una notificación para un usuario
    /// </summary>
    public class Notification : BaseEntity
    {
        /// <summary>
        /// ID del usuario que recibe la notificación
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tipo de notificación
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Título de la notificación
        /// </summary>
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Title { get; set; }

        /// <summary>
        /// Mensaje de la notificación
        /// </summary>
        [StringLength(500, ErrorMessage = "El mensaje no puede exceder 500 caracteres")]
        public string Message { get; set; }

        /// <summary>
        /// URL de acción (donde dirigir al usuario al hacer clic)
        /// </summary>
        [Url(ErrorMessage = "La URL de acción no es válida")]
        public string ActionUrl { get; set; }

        /// <summary>
        /// URL de imagen/icono para la notificación
        /// </summary>
        [Url(ErrorMessage = "La URL de imagen no es válida")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Indica si ha sido leída
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Fecha de lectura
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Indica si ha sido enviada por email
        /// </summary>
        public bool EmailSent { get; set; }

        /// <summary>
        /// Fecha de envío por email
        /// </summary>
        public DateTime? EmailSentAt { get; set; }

        /// <summary>
        /// Indica si ha sido enviada como push
        /// </summary>
        public bool PushSent { get; set; }

        /// <summary>
        /// Fecha de envío push
        /// </summary>
        public DateTime? PushSentAt { get; set; }

        // Referencias a entidades relacionadas
        public Guid? RelatedUserId { get; set; }
        public Guid? RelatedPostId { get; set; }
        public Guid? RelatedCommentId { get; set; }
        public Guid? RelatedMessageId { get; set; }
        public Guid? RelatedSubscriptionId { get; set; }
        public Guid? RelatedTipId { get; set; }

        /// <summary>
        /// Datos adicionales en formato JSON
        /// </summary>
        public string MetadataJson { get; set; }

        /// <summary>
        /// Prioridad de la notificación
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// Fecha de expiración de la notificación
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        // Relaciones
        public virtual User User { get; set; }
        public virtual User RelatedUser { get; set; }
        public virtual Post RelatedPost { get; set; }
        public virtual Comment RelatedComment { get; set; }
        public virtual Message RelatedMessage { get; set; }
        public virtual Subscription RelatedSubscription { get; set; }
        public virtual Tip RelatedTip { get; set; }
    }
}
