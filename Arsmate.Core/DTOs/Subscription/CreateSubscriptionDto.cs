using System;
using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.DTOs.Subscription
{
    /// <summary>
    /// DTO para crear una suscripción
    /// </summary>
    public class CreateSubscriptionDto
    {
        [Required(ErrorMessage = "El ID del creador es obligatorio")]
        public Guid CreatorId { get; set; }

        public bool AutoRenew { get; set; } = true;

        public string PromoCode { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public string PaymentMethodId { get; set; }
    }
}