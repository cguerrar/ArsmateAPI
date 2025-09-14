using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Enums;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de notificaciones
    /// </summary>
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(Guid userId, NotificationType type, string title, string message, object metadata = null);
        Task<bool> SendNotificationAsync(NotificationDto notification);
        Task<bool> SendEmailNotificationAsync(Guid userId, string subject, string body);
        Task<bool> SendPushNotificationAsync(Guid userId, string title, string message);
        Task<PaginatedResultDto<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
        Task<bool> DeleteAllNotificationsAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> UpdateNotificationSettingsAsync(Guid userId, bool emailEnabled, bool pushEnabled);
        Task<bool> RegisterPushTokenAsync(Guid userId, string token);
        Task<bool> UnregisterPushTokenAsync(Guid userId, string token);
        Task<bool> SendBulkNotificationAsync(List<Guid> userIds, NotificationType type, string title, string message);
        Task<bool> CleanupOldNotificationsAsync(int daysToKeep = 30);
    }
}