using System;
using Arsmate.Core.DTOs.User;

namespace Arsmate.Core.DTOs.Subscription
{
    /// <summary>
    /// DTO para información de suscripción
    /// </summary>
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid SubscriberId { get; set; }
        public Guid CreatorId { get; set; }
        public UserDto Subscriber { get; set; }
        public UserDto Creator { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public bool IsActive { get; set; }
        public bool AutoRenew { get; set; }
        public decimal PriceAtSubscription { get; set; }
        public string Currency { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? FreeDays { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}