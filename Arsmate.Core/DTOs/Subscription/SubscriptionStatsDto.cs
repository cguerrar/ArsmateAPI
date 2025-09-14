using System;

namespace Arsmate.Core.DTOs.Subscription
{
    /// <summary>
    /// DTO para estadísticas de suscripciones
    /// </summary>
    public class SubscriptionStatsDto
    {
        public int TotalSubscribers { get; set; }
        public int ActiveSubscribers { get; set; }
        public int NewSubscribersThisMonth { get; set; }
        public int CancelledThisMonth { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AverageSubscriptionPrice { get; set; }
        public decimal ChurnRate { get; set; }
        public decimal LifetimeValue { get; set; }

        // Tendencias
        public int SubscriberGrowthPercentage { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }

        // Top suscriptores
        public List<TopSubscriberDto> TopSubscribers { get; set; }
    }

    public class TopSubscriberDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public decimal TotalSpent { get; set; }
        public int MonthsSubscribed { get; set; }
        public DateTime SubscribedSince { get; set; }
    }
}