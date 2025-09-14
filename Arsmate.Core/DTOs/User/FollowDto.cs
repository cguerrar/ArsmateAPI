using System;

namespace Arsmate.Core.DTOs.User
{
    /// <summary>
    /// DTO para información de seguimiento
    /// </summary>
    public class FollowDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool IsVerified { get; set; }
        public bool IsCreator { get; set; }
        public DateTime FollowedAt { get; set; }
        public bool IsFollowingBack { get; set; }
    }
}