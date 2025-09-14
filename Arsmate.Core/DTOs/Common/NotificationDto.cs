using System;
using Arsmate.Core.Enums;

namespace Arsmate.Core.DTOs.Common
{
    /// <summary>
    /// DTO para notificaciones
    /// </summary>
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ActionUrl { get; set; }
        public string ImageUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public NotificationPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Referencias relacionadas
        public Guid? RelatedUserId { get; set; }
        public Guid? RelatedPostId { get; set; }
        public object Metadata { get; set; }
    }
}