using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Arsmate.Core.Enums;

namespace Arsmate.Core.DTOs.Post
{
    /// <summary>
    /// DTO para crear una nueva publicación
    /// </summary>
    public class CreatePostDto
    {
        [StringLength(2000, ErrorMessage = "El caption no puede exceder 2000 caracteres")]
        public string Caption { get; set; }

        [Required(ErrorMessage = "El tipo de publicación es obligatorio")]
        public PostType Type { get; set; }

        [Required(ErrorMessage = "La visibilidad es obligatoria")]
        public PostVisibility Visibility { get; set; }

        [Range(0, 9999.99, ErrorMessage = "El precio debe estar entre 0 y 9999.99")]
        public decimal? Price { get; set; }

        public bool CommentsEnabled { get; set; } = true;

        public DateTime? ScheduledAt { get; set; }

        public List<IFormFile> MediaFiles { get; set; }

        public List<string> Tags { get; set; }

        public List<Guid> MentionedUserIds { get; set; }
    }
}