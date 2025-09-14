using System;
using System.Collections.Generic;
using Arsmate.Core.DTOs.User;
using Arsmate.Core.DTOs.Post;

namespace Arsmate.Core.DTOs.Message
{
    /// <summary>
    /// DTO para mensajes
    /// </summary>
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public UserDto Sender { get; set; }
        public UserDto Recipient { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public decimal? Price { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public bool IsTipMessage { get; set; }
        public decimal? TipAmount { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MediaFileDto> Attachments { get; set; }

        public static implicit operator MessageDto?(string? v)
        {
            throw new NotImplementedException();
        }
    }
}