using System;
using System.Collections.Generic;
using Arsmate.Core.DTOs.User;

namespace Arsmate.Core.DTOs.Post
{
    /// <summary>
    /// DTO para comentarios
    /// </summary>
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public UserDto User { get; set; }
        public string Content { get; set; }
        public Guid? ParentCommentId { get; set; }
        public int LikesCount { get; set; }
        public int RepliesCount { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }

        // Estado del usuario actual
        public bool IsLiked { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        // Respuestas (si se solicitan)
        public List<CommentDto> Replies { get; set; }
    }
}