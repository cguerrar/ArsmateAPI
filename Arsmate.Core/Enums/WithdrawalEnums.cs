namespace Arsmate.Core.Enums
{
    /// <summary>
    /// Estados de los retiros
    /// </summary>
    public enum WithdrawalStatus
    {
        /// <summary>
        /// Pendiente de revisión
        /// </summary>
        Pending = 0,

        /// <summary>
        /// En proceso
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Completado
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Rechazado
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Cancelado por el usuario
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// En espera de información
        /// </summary>
        OnHold = 5,

        /// <summary>
        /// Falló el procesamiento
        /// </summary>
        Failed = 6
    }

    /// <summary>
    /// Métodos de retiro disponibles
    /// </summary>
    public enum WithdrawalMethod
    {
        /// <summary>
        /// Transferencia bancaria
        /// </summary>
        BankTransfer = 0,

        /// <summary>
        /// PayPal
        /// </summary>
        PayPal = 1,

        /// <summary>
        /// Stripe
        /// </summary>
        Stripe = 2,

        /// <summary>
        /// Criptomoneda
        /// </summary>
        Cryptocurrency = 3,

        /// <summary>
        /// Cheque
        /// </summary>
        Check = 4,

        /// <summary>
        /// Transferencia wire
        /// </summary>
        WireTransfer = 5,

        /// <summary>
        /// Mercado Pago
        /// </summary>
        MercadoPago = 6
    }
}
