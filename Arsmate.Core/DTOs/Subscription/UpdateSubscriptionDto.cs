namespace Arsmate.Core.DTOs.Subscription
{
    /// <summary>
    /// DTO para actualizar una suscripción
    /// </summary>
    public class UpdateSubscriptionDto
    {
        public bool? AutoRenew { get; set; }
        public string PaymentMethodId { get; set; }
    }
}