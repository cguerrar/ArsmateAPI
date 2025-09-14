using System;
using System.Collections.Generic;
using Arsmate.Core.DTOs.User;
using Arsmate.Core.Enums;

namespace Arsmate.Core.DTOs.Post
{
    /// <summary>
    /// DTO para información de publicación
    /// </summary>
    public class PostDto
    {
        public Guid Id { get; set; }
        public Guid CreatorId { get; set; }
        public UserDto Creator { get; set; }
        public string Caption { get; set; }
        public PostType Type { get; set; }
        public PostVisibility Visibility { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; }
        public bool IsArchived { get; set; }
        public bool CommentsEnabled { get; set; }
        public bool IsPinned { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int ViewsCount { get; set; }
        public int SharesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Estado del usuario actual
        public bool IsLiked { get; set; }
        public bool IsPurchased { get; set; }
        public bool CanView { get; set; }

        // Media files
        public List<MediaFileDto> MediaFiles { get; set; } = new List<MediaFileDto>();
    }
}