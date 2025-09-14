// ========================================
// Archivo: Arsmate.Infrastructure/Services/NotificationService.cs
// ========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Entities;
using Arsmate.Core.Enums;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;

namespace Arsmate.Infrastructure.Services
{
    /// <summary>
    /// Servicio de notificaciones sin dependencia directa de SignalR
    /// SignalR se maneja a través de un evento o interfaz
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ArsmateDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        // Interfaz opcional para notificaciones en tiempo real
        private readonly IRealtimeNotificationService _realtimeService;

        public NotificationService(
            ArsmateDbContext context,
            IEmailService emailService,
            ILogger<NotificationService> logger,
            IRealtimeNotificationService realtimeService = null)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _realtimeService = realtimeService;
        }

        public async Task<NotificationDto> CreateNotificationAsync(
            Guid userId,
            NotificationType type,
            string title,
            string message,
            object metadata = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                MetadataJson = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null,
                Priority = GetPriorityForType(type),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            var dto = new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                ActionUrl = notification.ActionUrl,
                ImageUrl = notification.ImageUrl,
                IsRead = notification.IsRead,
                Priority = notification.Priority,
                CreatedAt = notification.CreatedAt,
                Metadata = metadata
            };

            // Send real-time notification si el servicio está disponible
            await SendNotificationAsync(dto);

            return dto;
        }

        public async Task<bool> SendNotificationAsync(NotificationDto notification)
        {
            try
            {
                // Enviar notificación en tiempo real si el servicio está disponible
                if (_realtimeService != null)
                {
                    await _realtimeService.SendToUserAsync(
                        notification.UserId.ToString(),
                        "ReceiveNotification",
                        notification);
                }

                // Check if user wants email notifications
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user != null && user.EmailNotifications)
                {
                    await SendEmailNotificationAsync(
                        notification.UserId,
                        notification.Title,
                        notification.Message);
                }

                // Check if user wants push notifications
                if (user != null && user.PushNotifications && !string.IsNullOrEmpty(user.PushNotificationToken))
                {
                    await SendPushNotificationAsync(
                        notification.UserId,
                        notification.Title,
                        notification.Message);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to user {notification.UserId}");
                return false;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Guid userId, string subject, string body)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.EmailNotifications)
                return false;

            return await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        public async Task<bool> SendPushNotificationAsync(Guid userId, string title, string message)
        {
            // TODO: Implementar push notifications con Firebase o similar
            _logger.LogInformation($"Sending push notification to user {userId}: {title}");
            return await Task.FromResult(true);
        }

        public async Task<PaginatedResultDto<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    ActionUrl = n.ActionUrl,
                    ImageUrl = n.ImageUrl,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    Priority = n.Priority,
                    CreatedAt = n.CreatedAt,
                    ExpiresAt = n.ExpiresAt
                })
                .ToListAsync();

            return new PaginatedResultDto<NotificationDto>(notifications, totalCount, page, pageSize);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllNotificationsAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> UpdateNotificationSettingsAsync(Guid userId, bool emailEnabled, bool pushEnabled)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.EmailNotifications = emailEnabled;
            user.PushNotifications = pushEnabled;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegisterPushTokenAsync(Guid userId, string token)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.PushNotificationToken = token;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnregisterPushTokenAsync(Guid userId, string token)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            if (user.PushNotificationToken == token)
            {
                user.PushNotificationToken = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> SendBulkNotificationAsync(
            List<Guid> userIds,
            NotificationType type,
            string title,
            string message)
        {
            var notifications = new List<Notification>();

            foreach (var userId in userIds)
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = type,
                    Title = title,
                    Message = message,
                    Priority = GetPriorityForType(type),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();

            // Send real-time notifications si el servicio está disponible
            if (_realtimeService != null)
            {
                foreach (var notification in notifications)
                {
                    await _realtimeService.SendToUserAsync(
                        notification.UserId.ToString(),
                        "ReceiveNotification",
                        new NotificationDto
                        {
                            Id = notification.Id,
                            UserId = notification.UserId,
                            Type = notification.Type,
                            Title = notification.Title,
                            Message = notification.Message,
                            Priority = notification.Priority,
                            CreatedAt = notification.CreatedAt
                        });
                }
            }

            return true;
        }

        public async Task<bool> CleanupOldNotificationsAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                .ToListAsync();

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Cleaned up {oldNotifications.Count} old notifications");
            return true;
        }

        private NotificationPriority GetPriorityForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.PaymentReceived => NotificationPriority.High,
                NotificationType.NewSubscriber => NotificationPriority.High,
                NotificationType.WithdrawalProcessed => NotificationPriority.High,
                NotificationType.ModerationWarning => NotificationPriority.Urgent,
                NotificationType.SystemAlert => NotificationPriority.Urgent,
                NotificationType.NewMessage => NotificationPriority.Normal,
                NotificationType.NewComment => NotificationPriority.Low,
                NotificationType.NewLike => NotificationPriority.Low,
                _ => NotificationPriority.Normal
            };
        }
    }
}

