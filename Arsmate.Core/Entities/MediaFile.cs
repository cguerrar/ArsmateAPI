using System;
using System.ComponentModel.DataAnnotations;
using Arsmate.Core.Enums;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa un archivo multimedia (imagen, video, audio)
    /// </summary>
    public class MediaFile : BaseEntity
    {
        /// <summary>
        /// ID de la publicación a la que pertenece (opcional)
        /// </summary>
        public Guid? PostId { get; set; }

        /// <summary>
        /// ID del mensaje al que pertenece (opcional)
        /// </summary>
        public Guid? MessageId { get; set; }

        /// <summary>
        /// URL del archivo en el storage
        /// </summary>
        [Required]
        [Url(ErrorMessage = "La URL del archivo no es válida")]
        public string FileUrl { get; set; }

        /// <summary>
        /// URL de la miniatura (para videos e imágenes)
        /// </summary>
        [Url(ErrorMessage = "La URL de la miniatura no es válida")]
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// URL de la versión borrosa (para preview de contenido pago)
        /// </summary>
        [Url(ErrorMessage = "La URL del preview no es válida")]
        public string BlurredUrl { get; set; }

        /// <summary>
        /// Tipo de archivo multimedia
        /// </summary>
        public MediaType Type { get; set; }

        /// <summary>
        /// Nombre original del archivo
        /// </summary>
        [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
        public string FileName { get; set; }

        /// <summary>
        /// Tipo MIME del archivo
        /// </summary>
        [StringLength(100, ErrorMessage = "El tipo MIME no puede exceder 100 caracteres")]
        public string MimeType { get; set; }

        /// <summary>
        /// Tamaño del archivo en bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Duración en segundos (para videos y audio)
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Ancho en píxeles (para imágenes y videos)
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Alto en píxeles (para imágenes y videos)
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Orden de visualización en la publicación
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Indica si el archivo ha sido procesado
        /// </summary>
        public bool IsProcessed { get; set; }

        // Relaciones
        public virtual Post Post { get; set; }
        public virtual Message Message { get; set; }
        public int Order { get; set; }
    }
}