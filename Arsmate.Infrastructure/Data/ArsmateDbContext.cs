using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Arsmate.Core.Entities;
using Arsmate.Core.Enums;

namespace Arsmate.Infrastructure.Data
{
    /// <summary>
    /// Contexto principal de la base de datos
    /// </summary>
    public class ArsmateDbContext : DbContext
    {
        public ArsmateDbContext(DbContextOptions<ArsmateDbContext> options) : base(options)
        {
        }

        // DbSets - Tablas de la base de datos
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<MediaFile> MediaFiles { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }
        public DbSet<Tip> Tips { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostPurchase> PostPurchases { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // Configuración de User
            // ========================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.CreatedAt);
                entity.HasIndex(u => u.IsCreator);
                entity.HasIndex(u => u.IsActive);

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.DisplayName)
                    .HasMaxLength(100);

                entity.Property(u => u.Bio)
                    .HasMaxLength(500);

                entity.Property(u => u.Location)
    .HasMaxLength(100)
    .IsRequired(false);

                entity.Property(u => u.WebsiteUrl)
                    .HasMaxLength(200);

                entity.Property(u => u.SubscriptionPrice)
                    .HasPrecision(18, 2);

                entity.Property(u => u.MessagePrice)
                    .HasPrecision(18, 2);

                entity.Property(u => u.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                // URLs de imágenes
                entity.Property(u => u.ProfileImageUrl)
    .HasMaxLength(500)
    .IsRequired(false);

                entity.Property(u => u.ProfilePictureUrl)
                    .HasMaxLength(500);

                entity.Property(u => u.CoverImageUrl)
                    .HasMaxLength(500);

                entity.Property(u => u.CoverPhotoUrl)
                    .HasMaxLength(500);

                // Tokens y seguridad
                entity.Property(u => u.EmailConfirmationToken)
                    .HasMaxLength(255);

                entity.Property(u => u.PasswordResetToken)
                    .HasMaxLength(255);

                entity.Property(u => u.RefreshToken)
    .HasMaxLength(500)
    .IsRequired(false);

                entity.Property(u => u.TwoFactorSecret)
                    .HasMaxLength(255);

                entity.Property(u => u.PushNotificationToken)
                    .HasMaxLength(500);

                // IPs y razones
                entity.Property(u => u.LastLoginIp)
                    .HasMaxLength(45);

                entity.Property(u => u.SuspensionReason)
                    .HasMaxLength(500);

                // Redes sociales
                entity.Property(u => u.InstagramUsername)
                    .HasMaxLength(50);

                entity.Property(u => u.TwitterUsername)
                    .HasMaxLength(50);

                entity.Property(u => u.TikTokUsername)
                    .HasMaxLength(50);

                entity.Property(u => u.YouTubeUrl)
                    .HasMaxLength(200);

                // Mensaje de bienvenida
                entity.Property(u => u.WelcomeMessage)
                    .HasMaxLength(1000);

                entity.Property(u => u.WelcomeMessageDiscount)
                    .HasPrecision(5, 2);

                // Relación uno a uno con Wallet
                entity.HasOne(u => u.Wallet)
                    .WithOne(w => w.User)
                    .HasForeignKey<Wallet>(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // Configuración de Post
            // ========================================
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasIndex(p => p.CreatorId);
                entity.HasIndex(p => p.CreatedAt);
                entity.HasIndex(p => new { p.CreatorId, p.Type });
                entity.HasIndex(p => p.Visibility);
                entity.HasIndex(p => p.IsArchived);

                entity.Property(p => p.Caption)
                    .HasMaxLength(2000);

                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.HasOne(p => p.Creator)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // Configuración de MediaFile
            // ========================================
            modelBuilder.Entity<MediaFile>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasIndex(m => m.PostId);
                entity.HasIndex(m => m.MessageId);

                entity.Property(m => m.FileUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(m => m.ThumbnailUrl)
                    .HasMaxLength(500);

                entity.Property(m => m.BlurredUrl)
                    .HasMaxLength(500);

                entity.Property(m => m.FileName)
                    .HasMaxLength(255);

                entity.Property(m => m.MimeType)
                    .HasMaxLength(100);

                entity.HasOne(m => m.Post)
                    .WithMany(p => p.MediaFiles)
                    .HasForeignKey(m => m.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Message)
                    .WithMany(msg => msg.Attachments)
                    .HasForeignKey(m => m.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // Configuración de Subscription
            // ========================================
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.HasIndex(s => new { s.SubscriberId, s.CreatorId }).IsUnique();
                entity.HasIndex(s => s.IsActive);
                entity.HasIndex(s => s.EndDate);
                entity.HasIndex(s => s.NextBillingDate);

                entity.Property(s => s.PriceAtSubscription)
                    .HasPrecision(18, 2);

                entity.Property(s => s.DiscountPercentage)
                    .HasPrecision(5, 2);

                entity.Property(s => s.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.Property(s => s.CancellationReason)
                    .HasMaxLength(500);

                entity.HasOne(s => s.Subscriber)
                    .WithMany(u => u.Subscriptions)
                    .HasForeignKey(s => s.SubscriberId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Creator)
                    .WithMany(u => u.Subscribers)
                    .HasForeignKey(s => s.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // Configuración de Message
            // ========================================
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasIndex(m => new { m.SenderId, m.RecipientId });
                entity.HasIndex(m => m.CreatedAt);
                entity.HasIndex(m => m.IsRead);

                entity.Property(m => m.Content)
                    .HasMaxLength(1000);

                entity.Property(m => m.Price)
                    .HasPrecision(18, 2);

                entity.Property(m => m.TipAmount)
                    .HasPrecision(18, 2);

                entity.HasOne(m => m.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Recipient)
                    .WithMany(u => u.ReceivedMessages)
                    .HasForeignKey(m => m.RecipientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.ReplyToMessage)
                    .WithMany()
                    .HasForeignKey(m => m.ReplyToMessageId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // Configuración de Transaction
            // ========================================
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.CreatedAt);
                entity.HasIndex(t => t.Type);
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.ExternalTransactionId);

                entity.Property(t => t.Amount)
                    .HasPrecision(18, 2);

                entity.Property(t => t.Fee)
                    .HasPrecision(18, 2);

                entity.Property(t => t.NetAmount)
                    .HasPrecision(18, 2);

                entity.Property(t => t.Currency)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                entity.Property(t => t.ExternalTransactionId)
                    .HasMaxLength(255);

                entity.Property(t => t.PaymentMethod)
                    .HasMaxLength(50);

                entity.Property(t => t.CardLast4)
                    .HasMaxLength(4);

                entity.Property(t => t.IpAddress)
                    .HasMaxLength(45);

                entity.Property(t => t.CountryCode)
                    .HasMaxLength(2);

                entity.Property(t => t.FailureReason)
                    .HasMaxLength(500);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Subscription)
                    .WithMany(s => s.Transactions)
                    .HasForeignKey(t => t.SubscriptionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // Configuración de Wallet
            // ========================================
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.HasIndex(w => w.UserId).IsUnique();

                entity.Property(w => w.Balance)
                    .HasPrecision(18, 2);

                entity.Property(w => w.PendingBalance)
                    .HasPrecision(18, 2);

                entity.Property(w => w.TotalEarned)
                    .HasPrecision(18, 2);

                entity.Property(w => w.TotalWithdrawn)
                    .HasPrecision(18, 2);

                entity.Property(w => w.TotalTipsReceived)
                    .HasPrecision(18, 2);

                entity.Property(w => w.TotalSubscriptionsEarned)
                    .HasPrecision(18, 2);

                entity.Property(w => w.TotalPPVEarned)
                    .HasPrecision(18, 2);

                entity.Property(w => w.MinimumWithdrawalAmount)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(20);

                entity.Property(w => w.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.Property(w => w.PayPalEmail)
                    .HasMaxLength(256);
            });

            // ========================================
            // Configuración de Withdrawal
            // ========================================
            modelBuilder.Entity<Withdrawal>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.HasIndex(w => w.WalletId);
                entity.HasIndex(w => w.Status);
                entity.HasIndex(w => w.CreatedAt);

                entity.Property(w => w.Amount)
                    .HasPrecision(18, 2);

                entity.Property(w => w.Fee)
                    .HasPrecision(18, 2);

                entity.Property(w => w.NetAmount)
                    .HasPrecision(18, 2);

                entity.Property(w => w.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.Property(w => w.TransactionReference)
                    .HasMaxLength(255);

                entity.Property(w => w.RejectionReason)
                    .HasMaxLength(500);

                entity.Property(w => w.Notes)
                    .HasMaxLength(500);

                entity.HasOne(w => w.Wallet)
                    .WithMany(wallet => wallet.Withdrawals)
                    .HasForeignKey(w => w.WalletId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(w => w.ProcessedByUser)
                    .WithMany()
                    .HasForeignKey(w => w.ProcessedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // Configuración de Tip
            // ========================================
            modelBuilder.Entity<Tip>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.HasIndex(t => new { t.SenderId, t.RecipientId });
                entity.HasIndex(t => t.CreatedAt);

                entity.Property(t => t.Amount)
                    .HasPrecision(18, 2);

                entity.Property(t => t.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.Property(t => t.Message)
                    .HasMaxLength(200);

                entity.HasOne(t => t.Sender)
                    .WithMany(u => u.TipsSent)
                    .HasForeignKey(t => t.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Recipient)
                    .WithMany(u => u.TipsReceived)
                    .HasForeignKey(t => t.RecipientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Post)
                    .WithMany()
                    .HasForeignKey(t => t.PostId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // Configuración de Like
            // ========================================
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(l => l.Id);

                entity.HasIndex(l => new { l.UserId, l.PostId }).IsUnique();
                entity.HasIndex(l => l.CreatedAt);

                entity.HasOne(l => l.User)
                    .WithMany(u => u.Likes)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(l => l.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // Configuración de Comment
            // ========================================
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasIndex(c => c.PostId);
                entity.HasIndex(c => c.CreatedAt);
                entity.HasIndex(c => c.IsHidden);

                entity.Property(c => c.Content)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(c => c.HiddenReason)
                    .HasMaxLength(200);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // Configuración de PostPurchase
            // ========================================
            modelBuilder.Entity<PostPurchase>(entity =>
            {
                entity.HasKey(pp => pp.Id);

                entity.HasIndex(pp => new { pp.UserId, pp.PostId }).IsUnique();
                entity.HasIndex(pp => pp.CreatedAt);

                entity.Property(pp => pp.PricePaid)
                    .HasPrecision(18, 2);

                entity.Property(pp => pp.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.HasOne(pp => pp.User)
                    .WithMany(u => u.PostPurchases)
                    .HasForeignKey(pp => pp.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pp => pp.Post)
                    .WithMany(p => p.Purchases)
                    .HasForeignKey(pp => pp.PostId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pp => pp.Transaction)
                    .WithMany()
                    .HasForeignKey(pp => pp.TransactionId)
                    .OnDelete(DeleteBehavior.NoAction); // Cambiado de SetNull a NoAction
            });

            // ========================================
            // Configuración de Notification
            // ========================================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.IsRead);
                entity.HasIndex(n => n.CreatedAt);
                entity.HasIndex(n => n.Type);

                entity.Property(n => n.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(n => n.Message)
                    .HasMaxLength(500);

                entity.Property(n => n.ActionUrl)
                    .HasMaxLength(500);

                entity.Property(n => n.ImageUrl)
                    .HasMaxLength(500);

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.RelatedUser)
                    .WithMany()
                    .HasForeignKey(n => n.RelatedUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(n => n.RelatedPost)
                    .WithMany()
                    .HasForeignKey(n => n.RelatedPostId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // Configuración de Report
            // ========================================
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasIndex(r => r.Status);
                entity.HasIndex(r => r.CreatedAt);
                entity.HasIndex(r => r.Type);
                entity.HasIndex(r => r.Priority);

                entity.Property(r => r.Description)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(r => r.ModeratorNotes)
                    .HasMaxLength(1000);

                entity.Property(r => r.ActionTaken)
                    .HasMaxLength(200);

                entity.HasOne(r => r.Reporter)
                    .WithMany(u => u.ReportsMade)
                    .HasForeignKey(r => r.ReporterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReportedUser)
                    .WithMany(u => u.ReportsReceived)
                    .HasForeignKey(r => r.ReportedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReportedPost)
                    .WithMany()
                    .HasForeignKey(r => r.ReportedPostId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.ReviewedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // Conversión de Enums a string - CORREGIDO
            // ========================================
            ConfigureEnumConversions(modelBuilder);

            // ========================================
            // Datos semilla (Seed Data)
            // ========================================
            // Temporalmente comentado para crear la migración inicial
            // SeedData(modelBuilder);
        }

        /// <summary>
        /// Configura la conversión de enums a string para todas las propiedades enum
        /// </summary>
        private void ConfigureEnumConversions(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType.IsEnum)
                    {
                        // Crear el tipo del convertidor específico para cada enum
                        var converterType = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
                        var converter = Activator.CreateInstance(converterType) as ValueConverter;
                        property.SetValueConverter(converter);
                    }
                }
            }
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Usuario administrador
            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var adminWalletId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var now = DateTime.UtcNow;

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                Username = "admin",
                Email = "admin@arsmate.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
                DisplayName = "Arsmate Admin",
                Bio = "Official Arsmate International Administrator",

                // URLs de imágenes (valores por defecto)
                ProfileImageUrl = "/images/default-avatar.png",
                ProfilePictureUrl = "/images/default-avatar.png",
                CoverImageUrl = "/images/default-cover.jpg",
                CoverPhotoUrl = "/images/default-cover.jpg",

                // Configuración de creador
                IsCreator = false,
                IsVerified = true,
                EmailConfirmed = true,
                IsActive = true,
                Currency = "USD",

                // Redes sociales (opcionales, pero las inicializamos)
                InstagramUsername = null,
                TwitterUsername = null,
                TikTokUsername = null,
                YouTubeUrl = null,

                // Mensajes y descuentos
                WelcomeMessage = "Welcome to Arsmate International",
                WelcomeMessageDiscount = 0,

                // Configuración de privacidad
                ShowActivityStatus = true,
                AllowMessages = true,
                ShowSubscriberCount = true,
                ShowMediaInProfile = true,
                ShowPostCount = true,

                // Notificaciones
                EmailNotifications = true,
                PushNotifications = false,
                PushNotificationToken = null,

                // Tokens
                RefreshToken = null,
                RefreshTokenExpiryTime = null,
                EmailConfirmationToken = null,
                EmailConfirmationTokenExpires = null,
                PasswordResetToken = null,
                PasswordResetTokenExpires = null,
                TwoFactorSecret = null,
                TwoFactorEnabled = false,

                // Estadísticas
                FollowersCount = 0,
                FollowingCount = 0,
                PostsCount = 0,
                LikesCount = 0,
                TotalLikesReceived = 0,
                ProfileViewsCount = 0,

                // Estado de la cuenta
                IsSuspended = false,
                SuspendedUntil = null,
                SuspensionReason = null,
                LastLoginAt = now,
                LastLoginIp = "127.0.0.1",

                // Otros
                Location = "Global",
                WebsiteUrl = "https://arsmate.com",
                DateOfBirth = null,
                SubscriptionPrice = null,
                MessagePrice = null,

                // Timestamps
                CreatedAt = now,
                UpdatedAt = now
            });

            modelBuilder.Entity<Wallet>().HasData(new Wallet
            {
                Id = adminWalletId,
                UserId = adminId,
                Balance = 0,
                PendingBalance = 0,
                Currency = "USD",
                TotalEarned = 0,
                TotalWithdrawn = 0,
                MinimumWithdrawalAmount = 20,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}