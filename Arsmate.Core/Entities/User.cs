using System;
using System.Collections.Generic;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Entidad de Usuario completa con todas las propiedades necesarias
    /// </summary>
    public class User : BaseEntity
    {
        // Información básica de autenticación
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool EmailConfirmed { get; set; }
        public string EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpires { get; set; }

        // Tokens de autenticación
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Información del perfil
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ProfilePictureUrl { get; set; } // Alias para compatibilidad
        public string CoverImageUrl { get; set; }
        public string CoverPhotoUrl { get; set; } // Alias para compatibilidad
        public DateTime? DateOfBirth { get; set; }
        public string Location { get; set; }
        public string WebsiteUrl { get; set; }

        // Redes sociales
        public string InstagramUsername { get; set; }
        public string TwitterUsername { get; set; }
        public string TikTokUsername { get; set; }
        public string YouTubeUrl { get; set; }

        // Configuración de creador
        public bool IsCreator { get; set; }
        public bool IsVerified { get; set; }
        public decimal? SubscriptionPrice { get; set; }
        public decimal? MessagePrice { get; set; }
        public string Currency { get; set; }
        public string WelcomeMessage { get; set; }
        public decimal? WelcomeMessageDiscount { get; set; }

        // Estadísticas
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostsCount { get; set; }
        public int LikesCount { get; set; }
        public int TotalLikesReceived { get; set; }
        public int ProfileViewsCount { get; set; }

        // Configuración de privacidad
        public bool ShowActivityStatus { get; set; }
        public bool AllowMessages { get; set; }
        public bool ShowSubscriberCount { get; set; }
        public bool ShowMediaInProfile { get; set; }
        public bool ShowPostCount { get; set; }

        // Configuración de notificaciones
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public string PushNotificationToken { get; set; }

        // Estado de la cuenta
        public bool IsActive { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime? SuspendedUntil { get; set; }
        public string SuspensionReason { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string LastLoginIp { get; set; }

        // Tokens de recuperación
        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }

        // Verificación de dos factores
        public bool TwoFactorEnabled { get; set; }
        public string TwoFactorSecret { get; set; }

        // Relaciones - Navegación
        public virtual Wallet Wallet { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual ICollection<Subscription> Subscribers { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; }
        public virtual ICollection<Message> ReceivedMessages { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Report> ReportsMade { get; set; }
        public virtual ICollection<Report> ReportsReceived { get; set; }
        public virtual ICollection<PostPurchase> PostPurchases { get; set; }
        public virtual ICollection<Tip> TipsSent { get; set; }
        public virtual ICollection<Tip> TipsReceived { get; set; }

        public User()
        {
            Posts = new HashSet<Post>();
            Subscriptions = new HashSet<Subscription>();
            Subscribers = new HashSet<Subscription>();
            SentMessages = new HashSet<Message>();
            ReceivedMessages = new HashSet<Message>();
            Transactions = new HashSet<Transaction>();
            Likes = new HashSet<Like>();
            Comments = new HashSet<Comment>();
            Notifications = new HashSet<Notification>();
            ReportsMade = new HashSet<Report>();
            ReportsReceived = new HashSet<Report>();
            PostPurchases = new HashSet<PostPurchase>();
            TipsSent = new HashSet<Tip>();
            TipsReceived = new HashSet<Tip>();

            // Valores por defecto
            IsActive = true;
            IsSuspended = false;
            EmailConfirmed = false;
            IsCreator = false;
            IsVerified = false;
            ShowActivityStatus = true;
            AllowMessages = true;
            ShowSubscriberCount = true;
            ShowMediaInProfile = true;
            ShowPostCount = true;
            EmailNotifications = true;
            PushNotifications = true;
            TwoFactorEnabled = false;
            Currency = "USD";
            FollowersCount = 0;
            FollowingCount = 0;
            PostsCount = 0;
            LikesCount = 0;
            TotalLikesReceived = 0;
            ProfileViewsCount = 0;
        }
    }
}