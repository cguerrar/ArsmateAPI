using System;
using Arsmate.Core.DTOs.User;

namespace Arsmate.Core.DTOs.Post
{
    /// <summary>
    /// DTO para likes
    /// </summary>
    public class LikeDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public UserDto User { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}