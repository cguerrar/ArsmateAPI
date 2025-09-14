namespace Arsmate.Core.Enums
{
    /// <summary>
    /// Tipos de transacciones financieras
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Pago de suscripción
        /// </summary>
        Subscription = 0,

        /// <summary>
        /// Renovación de suscripción
        /// </summary>
        SubscriptionRenewal = 1,

        /// <summary>
        /// Propina
        /// </summary>
        Tip = 2,

        /// <summary>
        /// Compra de publicación PPV
        /// </summary>
        PostPurchase = 3,

        /// <summary>
        /// Compra de mensaje
        /// </summary>
        MessagePurchase = 4,

        /// <summary>
        /// Retiro de fondos
        /// </summary>
        Withdrawal = 5,

        /// <summary>
        /// Reembolso
        /// </summary>
        Refund = 6,

        /// <summary>
        /// Depósito de fondos
        /// </summary>
        Deposit = 7,

        /// <summary>
        /// Comisión de la plataforma
        /// </summary>
        PlatformFee = 8,

        /// <summary>
        /// Ajuste manual
        /// </summary>
        Adjustment = 9,

        /// <summary>
        /// Pago por referido
        /// </summary>
        ReferralBonus = 10
    }

    /// <summary>
    /// Estados de las transacciones
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// Pendiente de procesamiento
        /// </summary>
        Pending = 0,

        /// <summary>
        /// En proceso
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Completada exitosamente
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Falló
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Cancelada
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Reembolsada
        /// </summary>
        Refunded = 5,

        /// <summary>
        /// En disputa
        /// </summary>
        Disputed = 6,

        /// <summary>
        /// Requiere verificación
        /// </summary>
        RequiresVerification = 7
    }
}
