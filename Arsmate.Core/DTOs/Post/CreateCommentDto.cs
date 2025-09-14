using System;
using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.DTOs.Post
{
    /// <summary>
    /// DTO para crear un comentario
    /// </summary>
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "El contenido es obligatorio")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El comentario debe tener entre 1 y 500 caracteres")]
        public string Content { get; set; }

        public Guid? ParentCommentId { get; set; }
    }
}