using System.ComponentModel.DataAnnotations;
using Arsmate.Core.Enums;

namespace Arsmate.Core.DTOs.Post
{
    /// <summary>
    /// DTO para actualizar una publicación
    /// </summary>
    public class UpdatePostDto
    {
        [StringLength(2000, ErrorMessage = "El caption no puede exceder 2000 caracteres")]
        public string Caption { get; set; }

        public PostVisibility? Visibility { get; set; }

        [Range(0, 9999.99, ErrorMessage = "El precio debe estar entre 0 y 9999.99")]
        public decimal? Price { get; set; }

        public bool? CommentsEnabled { get; set; }

        public bool? IsArchived { get; set; }

        public bool? IsPinned { get; set; }
    }
}