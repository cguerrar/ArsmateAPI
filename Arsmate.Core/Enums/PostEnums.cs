namespace Arsmate.Core.Enums
{
    /// <summary>
    /// Tipos de publicación disponibles
    /// </summary>
    public enum PostType
    {
        /// <summary>
        /// Publicación con fotos
        /// </summary>
        Photo = 0,

        /// <summary>
        /// Publicación con video
        /// </summary>
        Video = 1,

        /// <summary>
        /// Publicación con audio
        /// </summary>
        Audio = 2,

        /// <summary>
        /// Publicación solo texto
        /// </summary>
        Text = 3,

        /// <summary>
        /// Encuesta
        /// </summary>
        Poll = 4,

        /// <summary>
        /// Transmisión en vivo
        /// </summary>
        LiveStream = 5,

        /// <summary>
        /// Historia temporal (24 horas)
        /// </summary>
        Story = 6,

        /// <summary>
        /// Publicación con múltiples tipos de media
        /// </summary>
        Mixed = 7
    }

    /// <summary>
    /// Niveles de visibilidad de las publicaciones
    /// </summary>
    public enum PostVisibility
    {
        /// <summary>
        /// Visible para todos
        /// </summary>
        Public = 0,

        /// <summary>
        /// Solo para suscriptores
        /// </summary>
        Subscribers = 1,

        /// <summary>
        /// Requiere pago adicional (PPV)
        /// </summary>
        PayPerView = 2,

        /// <summary>
        /// Solo el creador puede verlo
        /// </summary>
        Private = 3,

        /// <summary>
        /// Solo seguidores (no suscriptores)
        /// </summary>
        Followers = 4,

        /// <summary>
        /// Lista personalizada de usuarios
        /// </summary>
        CustomList = 5
    }
}