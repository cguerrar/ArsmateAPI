using System;

namespace Arsmate.Core.DTOs.User
{
    /// <summary>
    /// DTO con información básica del usuario
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string CoverPhotoUrl { get; set; }
        public bool IsCreator { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public decimal? SubscriptionPrice { get; set; }
        public string Currency { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}