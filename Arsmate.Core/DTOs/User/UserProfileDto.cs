using System;

namespace Arsmate.Core.DTOs.User
{
    /// <summary>
    /// DTO con información completa del perfil de usuario
    /// </summary>
    public class UserProfileDto : UserDto
    {
        public string Location { get; set; }
        public string WebsiteUrl { get; set; }
        public bool ShowPostCount { get; set; }
        public bool AllowMessages { get; set; }
        public decimal? MessagePrice { get; set; }
        public string WelcomeMessage { get; set; }
        public int? WelcomeMessageDiscount { get; set; }

        // Redes sociales
        public string TwitterUsername { get; set; }
        public string InstagramUsername { get; set; }
        public string TikTokUsername { get; set; }
        public string YouTubeUrl { get; set; }

        // Estadísticas adicionales
        public int TotalLikesReceived { get; set; }
        public int ProfileViewsCount { get; set; }
        public decimal TotalEarned { get; set; }

        // Estado de suscripción (si aplica)
        public bool IsSubscribed { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
        public bool HasActiveSubscription { get; set; }
    }
}