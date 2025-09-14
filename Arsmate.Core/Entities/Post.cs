using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Arsmate.Core.Enums;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa una publicación de contenido
    /// </summary>
    public class Post : BaseEntity
    {
        /// <summary>
        /// ID del creador de la publicación
        /// </summary>
        [Required]
        public Guid CreatorId { get; set; }

        /// <summary>
        /// Texto o descripción de la publicación
        /// </summary>
        [StringLength(2000, ErrorMessage = "El caption no puede exceder 2000 caracteres")]
        public string Caption { get; set; }

        /// <summary>
        /// Tipo de publicación (foto, video, texto, etc.)
        /// </summary>
        public PostType Type { get; set; }

        /// <summary>
        /// Visibilidad de la publicación
        /// </summary>
        public PostVisibility Visibility { get; set; }

        /// <summary>
        /// Precio para contenido Pay-Per-View (null si es gratis)
        /// </summary>
        [Range(0, 9999.99, ErrorMessage = "El precio debe estar entre 0 y 9999.99")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Indica si la publicación está archivada
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Indica si los comentarios están habilitados
        /// </summary>
        public bool CommentsEnabled { get; set; } = true;

        /// <summary>
        /// Indica si la publicación está fijada en el perfil
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Fecha programada de publicación (null si se publica inmediatamente)
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Número de likes
        /// </summary>
        public int LikesCount { get; set; }

        /// <summary>
        /// Número de comentarios
        /// </summary>
        public int CommentsCount { get; set; }

        /// <summary>
        /// Número de vistas
        /// </summary>
        public int ViewsCount { get; set; }

        /// <summary>
        /// Número de veces compartido
        /// </summary>
        public int SharesCount { get; set; }

        // Relaciones
        public virtual User Creator { get; set; }
        public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostPurchase> Purchases { get; set; } = new List<PostPurchase>();
    }
}