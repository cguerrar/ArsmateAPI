using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.DTOs.Subscription
{
    /// <summary>
    /// DTO para cancelar una suscripción
    /// </summary>
    public class CancelSubscriptionDto
    {
        [StringLength(500, ErrorMessage = "La razón no puede exceder 500 caracteres")]
        public string Reason { get; set; }

        public bool CancelImmediately { get; set; }
    }
}