using Arsmate.Core.Entities;

namespace Arsmate.Api.DTOs.Auth
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public string Bio { get; set; }
        public bool IsCreator { get; set; }
        public bool IsVerified { get; set; }
        public bool EmailConfirmed { get; set; }

        // Campos de creador
        public decimal? SubscriptionPrice { get; set; }
        public decimal? MessagePrice { get; set; }
        public string Currency { get; set; }

        // Estadísticas
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostsCount { get; set; }

        // Métodos de conversión
        public static UserDto FromEntity(User user)
        {
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName ?? user.Username,
                ProfileImageUrl = user.ProfileImageUrl ?? user.ProfilePictureUrl ?? "/images/default-avatar.png",
                CoverImageUrl = user.CoverImageUrl ?? user.CoverPhotoUrl ?? "/images/default-cover.jpg",
                Bio = user.Bio,
                IsCreator = user.IsCreator,
                IsVerified = user.IsVerified,
                EmailConfirmed = user.EmailConfirmed,
                SubscriptionPrice = user.SubscriptionPrice,
                MessagePrice = user.MessagePrice,
                Currency = user.Currency ?? "USD",
                FollowersCount = user.FollowersCount,
                FollowingCount = user.FollowingCount,
                PostsCount = user.PostsCount
            };
        }
    }
}
