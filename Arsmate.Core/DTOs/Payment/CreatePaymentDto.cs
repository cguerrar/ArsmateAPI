using System;
using System.ComponentModel.DataAnnotations;
using Arsmate.Core.Enums;

namespace Arsmate.Core.DTOs.Payment
{
    /// <summary>
    /// DTO para crear un pago
    /// </summary>
    public class CreatePaymentDto
    {
        [Required(ErrorMessage = "El tipo de transacción es obligatorio")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe estar entre 0.01 y 999999.99")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La moneda es obligatoria")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "El código de moneda debe ser de 3 caracteres")]
        public string Currency { get; set; } = "USD";

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public string PaymentMethodId { get; set; }

        public Guid? SubscriptionId { get; set; }
        public Guid? PostId { get; set; }
        public Guid? MessageId { get; set; }
        public Guid? TipRecipientId { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Description { get; set; }
    }
}