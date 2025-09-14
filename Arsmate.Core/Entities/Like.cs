using System;
using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa un "me gusta" en una publicación
    /// </summary>
    public class Like : BaseEntity
    {
        /// <summary>
        /// ID del usuario que da el like
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// ID de la publicación
        /// </summary>
        [Required]
        public Guid PostId { get; set; }

        /// <summary>
        /// Indica si el creador ha sido notificado
        /// </summary>
        public bool CreatorNotified { get; set; }

        // Relaciones
        public virtual User User { get; set; }
        public virtual Post Post { get; set; }
    }
}
