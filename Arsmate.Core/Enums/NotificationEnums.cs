namespace Arsmate.Core.Enums
{
    /// <summary>
    /// Tipos de notificaciones
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Nuevo suscriptor
        /// </summary>
        NewSubscriber = 0,

        /// <summary>
        /// Suscripción por expirar
        /// </summary>
        SubscriptionExpiring = 1,

        /// <summary>
        /// Suscripción expirada
        /// </summary>
        SubscriptionExpired = 2,

        /// <summary>
        /// Nueva publicación de creador seguido
        /// </summary>
        NewPost = 3,

        /// <summary>
        /// Nuevo mensaje
        /// </summary>
        NewMessage = 4,

        /// <summary>
        /// Nuevo comentario
        /// </summary>
        NewComment = 5,

        /// <summary>
        /// Nuevo like
        /// </summary>
        NewLike = 6,

        /// <summary>
        /// Nueva propina recibida
        /// </summary>
        NewTip = 7,

        /// <summary>
        /// Pago recibido
        /// </summary>
        PaymentReceived = 8,

        /// <summary>
        /// Retiro procesado
        /// </summary>
        WithdrawalProcessed = 9,

        /// <summary>
        /// Alerta del sistema
        /// </summary>
        SystemAlert = 10,

        /// <summary>
        /// Nuevo seguidor
        /// </summary>
        NewFollower = 11,

        /// <summary>
        /// Mención en comentario
        /// </summary>
        MentionInComment = 12,

        /// <summary>
        /// Mención en publicación
        /// </summary>
        MentionInPost = 13,

        /// <summary>
        /// Contenido comprado
        /// </summary>
        ContentPurchased = 14,

        /// <summary>
        /// Transmisión en vivo iniciada
        /// </summary>
        LiveStreamStarted = 15,

        /// <summary>
        /// Promoción especial
        /// </summary>
        SpecialPromotion = 16,

        /// <summary>
        /// Verificación completada
        /// </summary>
        VerificationCompleted = 17,

        /// <summary>
        /// Advertencia de moderación
        /// </summary>
        ModerationWarning = 18
    }

    /// <summary>
    /// Prioridad de las notificaciones
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// Baja prioridad
        /// </summary>
        Low = 0,

        /// <summary>
        /// Prioridad normal
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Alta prioridad
        /// </summary>
        High = 2,

        /// <summary>
        /// Urgente
        /// </summary>
        Urgent = 3
    }
}