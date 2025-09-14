using System;
using System.ComponentModel.DataAnnotations;
using Arsmate.Core.Enums;

namespace Arsmate.Core.Entities
{
    /// <summary>
    /// Representa un reporte de contenido o usuario inapropiado
    /// </summary>
    public class Report : BaseEntity
    {
        /// <summary>
        /// ID del usuario que hace el reporte
        /// </summary>
        [Required]
        public Guid ReporterId { get; set; }

        /// <summary>
        /// Tipo de reporte
        /// </summary>
        public ReportType Type { get; set; }

        /// <summary>
        /// Razón del reporte
        /// </summary>
        public ReportReason Reason { get; set; }

        /// <summary>
        /// Descripción detallada del reporte
        /// </summary>
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 1000 caracteres")]
        public string Description { get; set; }

        /// <summary>
        /// Estado del reporte
        /// </summary>
        public ReportStatus Status { get; set; }

        /// <summary>
        /// Prioridad del reporte
        /// </summary>
        public ReportPriority Priority { get; set; } = ReportPriority.Normal;

        // Referencias a entidades reportadas
        public Guid? ReportedUserId { get; set; }
        public Guid? ReportedPostId { get; set; }
        public Guid? ReportedCommentId { get; set; }
        public Guid? ReportedMessageId { get; set; }

        /// <summary>
        /// ID del moderador que revisa el reporte
        /// </summary>
        public Guid? ReviewedByUserId { get; set; }

        /// <summary>
        /// Fecha de revisión
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Notas del moderador
        /// </summary>
        [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
        public string ModeratorNotes { get; set; }

        /// <summary>
        /// Acción tomada
        /// </summary>
        [StringLength(200, ErrorMessage = "La acción tomada no puede exceder 200 caracteres")]
        public string ActionTaken { get; set; }

        /// <summary>
        /// Fecha de resolución
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Evidencia adjunta (URLs de imágenes, etc.)
        /// </summary>
        public string EvidenceUrls { get; set; }

        /// <summary>
        /// Indica si el reportador fue notificado de la resolución
        /// </summary>
        public bool ReporterNotified { get; set; }

        /// <summary>
        /// Indica si el usuario reportado fue notificado
        /// </summary>
        public bool ReportedUserNotified { get; set; }

        // Relaciones
        public virtual User Reporter { get; set; }
        public virtual User ReportedUser { get; set; }
        public virtual Post ReportedPost { get; set; }
        public virtual Comment ReportedComment { get; set; }
        public virtual Message ReportedMessage { get; set; }
        public virtual User ReviewedByUser { get; set; }
    }
}