namespace Arsmate.Core.Enums
{
    /// <summary>
    /// Tipos de reportes
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Reporte de usuario
        /// </summary>
        User = 0,

        /// <summary>
        /// Reporte de publicación
        /// </summary>
        Post = 1,

        /// <summary>
        /// Reporte de comentario
        /// </summary>
        Comment = 2,

        /// <summary>
        /// Reporte de mensaje
        /// </summary>
        Message = 3,

        /// <summary>
        /// Reporte de transmisión en vivo
        /// </summary>
        LiveStream = 4,

        /// <summary>
        /// Reporte de historia
        /// </summary>
        Story = 5
    }

    /// <summary>
    /// Razones para reportar
    /// </summary>
    public enum ReportReason
    {
        /// <summary>
        /// Spam
        /// </summary>
        Spam = 0,

        /// <summary>
        /// Acoso o bullying
        /// </summary>
        HarassmentOrBullying = 1,

        /// <summary>
        /// Discurso de odio
        /// </summary>
        HateSpeech = 2,

        /// <summary>
        /// Desnudez inapropiada
        /// </summary>
        Nudity = 3,

        /// <summary>
        /// Violencia
        /// </summary>
        Violence = 4,

        /// <summary>
        /// Información falsa
        /// </summary>
        FalseInformation = 5,

        /// <summary>
        /// Violación de propiedad intelectual
        /// </summary>
        IntellectualProperty = 6,

        /// <summary>
        /// Contenido ilegal
        /// </summary>
        IllegalContent = 7,

        /// <summary>
        /// Suplantación de identidad
        /// </summary>
        Impersonation = 8,

        /// <summary>
        /// Menor de edad
        /// </summary>
        Underage = 9,

        /// <summary>
        /// Autolesión o suicidio
        /// </summary>
        SelfHarm = 10,

        /// <summary>
        /// Terrorismo
        /// </summary>
        Terrorism = 11,

        /// <summary>
        /// Otro
        /// </summary>
        Other = 12
    }

    /// <summary>
    /// Estado del reporte
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>
        /// Pendiente de revisión
        /// </summary>
        Pending = 0,

        /// <summary>
        /// En revisión
        /// </summary>
        UnderReview = 1,

        /// <summary>
        /// Resuelto
        /// </summary>
        Resolved = 2,

        /// <summary>
        /// Desestimado
        /// </summary>
        Dismissed = 3,

        /// <summary>
        /// Escalado
        /// </summary>
        Escalated = 4,

        /// <summary>
        /// Requiere más información
        /// </summary>
        RequiresMoreInfo = 5
    }

    /// <summary>
    /// Prioridad del reporte
    /// </summary>
    public enum ReportPriority
    {
        /// <summary>
        /// Baja
        /// </summary>
        Low = 0,

        /// <summary>
        /// Normal
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Alta
        /// </summary>
        High = 2,

        /// <summary>
        /// Crítica
        /// </summary>
        Critical = 3
    }
}
