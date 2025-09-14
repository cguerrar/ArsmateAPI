namespace Arsmate.Core.Enums
{
    /// <summary>
    /// Roles de usuario en el sistema
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Usuario regular
        /// </summary>
        User = 0,

        /// <summary>
        /// Creador de contenido
        /// </summary>
        Creator = 1,

        /// <summary>
        /// Moderador
        /// </summary>
        Moderator = 2,

        /// <summary>
        /// Administrador
        /// </summary>
        Administrator = 3,

        /// <summary>
        /// Super administrador
        /// </summary>
        SuperAdmin = 4
    }

    /// <summary>
    /// Estado de verificación del usuario
    /// </summary>
    public enum VerificationStatus
    {
        /// <summary>
        /// No verificado
        /// </summary>
        NotVerified = 0,

        /// <summary>
        /// Pendiente de verificación
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Verificado
        /// </summary>
        Verified = 2,

        /// <summary>
        /// Rechazado
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Requiere más información
        /// </summary>
        RequiresMoreInfo = 4
    }
}