using System;
using Arsmate.Core.DTOs.User;

namespace Arsmate.Core.DTOs.Message
{
    /// <summary>
    /// DTO para conversaciones
    /// </summary>
    public class ConversationDto
    {
        public Guid UserId { get; set; }
        public UserDto User { get; set; }
        public MessageDto LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public DateTime LastMessageAt { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public object AvatarUrl { get; set; }
    }
}