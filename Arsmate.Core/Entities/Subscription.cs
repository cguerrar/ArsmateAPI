using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa una suscripción entre un usuario y un creador
    /// </summary>
    public class Subscription : BaseEntity
    {
        /// <summary>
        /// ID del usuario que se suscribe
        /// </summary>
        [Required]
        public Guid SubscriberId { get; set; }

        /// <summary>
        /// ID del creador al que se suscribe
        /// </summary>
        [Required]
        public Guid CreatorId { get; set; }

        /// <summary>
        /// Fecha de inicio de la suscripción
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Fecha de finalización de la suscripción
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Fecha del próximo pago
        /// </summary>
        public DateTime? NextBillingDate { get; set; }

        /// <summary>
        /// Indica si la suscripción está activa
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indica si se renueva automáticamente
        /// </summary>
        public bool AutoRenew { get; set; } = true;

        /// <summary>
        /// Precio pagado en el momento de la suscripción
        /// </summary>
        [Range(0, 9999.99, ErrorMessage = "El precio debe estar entre 0 y 9999.99")]
        public decimal PriceAtSubscription { get; set; }

        /// <summary>
        /// Moneda de la suscripción
        /// </summary>
        [StringLength(3, ErrorMessage = "El código de moneda debe ser de 3 caracteres")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Porcentaje de descuento aplicado
        /// </summary>
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal? DiscountPercentage { get; set; }

        /// <summary>
        /// Días gratis otorgados
        /// </summary>
        [Range(0, 365, ErrorMessage = "Los días gratis deben estar entre 0 y 365")]
        public int? FreeDays { get; set; }

        /// <summary>
        /// Razón de cancelación
        /// </summary>
        [StringLength(500, ErrorMessage = "La razón de cancelación no puede exceder 500 caracteres")]
        public string CancellationReason { get; set; }

        /// <summary>
        /// Fecha de cancelación
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Indica si el creador ha sido notificado
        /// </summary>
        public bool CreatorNotified { get; set; }

        /// <summary>
        /// Indica si el suscriptor ha sido notificado
        /// </summary>
        public bool SubscriberNotified { get; set; }

        // Relaciones
        public virtual User Subscriber { get; set; }
        public virtual User Creator { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
} 