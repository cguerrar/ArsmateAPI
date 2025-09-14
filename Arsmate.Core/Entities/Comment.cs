using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa un comentario en una publicación
    /// </summary>
    public class Comment : BaseEntity
    {
        /// <summary>
        /// ID del usuario que comenta
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// ID de la publicación
        /// </summary>
        [Required]
        public Guid PostId { get; set; }

        /// <summary>
        /// Contenido del comentario
        /// </summary>
        [Required(ErrorMessage = "El comentario no puede estar vacío")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El comentario debe tener entre 1 y 500 caracteres")]
        public string Content { get; set; }

        /// <summary>
        /// ID del comentario padre (para respuestas)
        /// </summary>
        public Guid? ParentCommentId { get; set; }

        /// <summary>
        /// Número de likes en el comentario
        /// </summary>
        public int LikesCount { get; set; }

        /// <summary>
        /// Número de respuestas
        /// </summary>
        public int RepliesCount { get; set; }

        /// <summary>
        /// Indica si el comentario ha sido editado
        /// </summary>
        public bool IsEdited { get; set; }

        /// <summary>
        /// Fecha de la última edición
        /// </summary>
        public DateTime? EditedAt { get; set; }

        /// <summary>
        /// Indica si el comentario está fijado por el creador
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Indica si el comentario ha sido ocultado
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Razón por la que fue ocultado
        /// </summary>
        [StringLength(200, ErrorMessage = "La razón no puede exceder 200 caracteres")]
        public string HiddenReason { get; set; }

        // Relaciones
        public virtual User User { get; set; }
        public virtual Post Post { get; set; }
        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}