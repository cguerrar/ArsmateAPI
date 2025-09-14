// DTOs/PostDtos.cs
using System.ComponentModel.DataAnnotations;

namespace ArsmateAPI.DTOs
{
    // DTO para crear un post
    public class CreatePostDto
    {
        [MaxLength(5000)]
        public string Content { get; set; }

        public List<MediaFileDto> Media { get; set; }

        [Required]
        [RegularExpression("^(public|followers|subscribers)$")]
        public string Visibility { get; set; }

        [Range(0.5, 10000)]
        public decimal? Price { get; set; }

        public List<string> Tags { get; set; }

        public DateTime? ScheduledAt { get; set; }
    }

    // DTO para archivos multimedia
    public class MediaFileDto
    {
        [Required]
        public string Filename { get; set; }

        [Required]
        public string MimeType { get; set; }

        [Required]
        [Range(1, 10485760)] // Max 10MB
        public int Size { get; set; }

        [Required]
        public string Data { get; set; } // Base64

        public string ThumbnailData { get; set; } // Base64

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? Duration { get; set; }
    }

    // DTO para respuesta de post
    public class PostResponseDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public string Visibility { get; set; }
        public decimal? Price { get; set; }
        public bool IsPaid { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int SharesCount { get; set; }
        public int ViewsCount { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserInfoDto User { get; set; }
        public List<MediaDto> Media { get; set; }
        public List<string> Tags { get; set; }
        public bool IsLiked { get; set; }
        public bool IsBookmarked { get; set; }
        public bool CanView { get; set; }
    }

    // DTO para información de usuario
    public class UserInfoDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsVerified { get; set; }
    }

    // DTO para media en respuesta
    public class MediaDto
    {
        public string Id { get; set; }
        public string Filename { get; set; }
        public string MimeType { get; set; }
        public int Size { get; set; }
        public string Data { get; set; } // Solo se incluye si CanView = true
        public string ThumbnailData { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; }
    }

    // DTO para respuesta del feed
    public class FeedResponseDto
    {
        public List<PostResponseDto> Posts { get; set; }
        public int Total { get; set; }
        public bool HasMore { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
    }

    // DTO para actualizar post
    public class UpdatePostDto
    {
        [MaxLength(5000)]
        public string Content { get; set; }

        [RegularExpression("^(public|followers|subscribers)$")]
        public string Visibility { get; set; }

        [Range(0.5, 10000)]
        public decimal? Price { get; set; }

        public List<string> Tags { get; set; }
    }
}