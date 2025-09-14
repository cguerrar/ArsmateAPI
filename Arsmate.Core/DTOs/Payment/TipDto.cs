using System;
using System.ComponentModel.DataAnnotations;
using Arsmate.Core.DTOs.User;

namespace Arsmate.Core.DTOs.Payment
{
    /// <summary>
    /// DTO para propinas
    /// </summary>
    public class TipDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public UserDto Sender { get; set; }
        public UserDto Recipient { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Message { get; set; }
        public Guid? PostId { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTipDto
    {
        [Required(ErrorMessage = "El destinatario es obligatorio")]
        public Guid RecipientId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 9999.99, ErrorMessage = "La propina debe estar entre 0.01 y 9999.99")]
        public decimal Amount { get; set; }

        [StringLength(200, ErrorMessage = "El mensaje no puede exceder 200 caracteres")]
        public string Message { get; set; }

        public Guid? PostId { get; set; }

        public bool IsAnonymous { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public string PaymentMethodId { get; set; }
    }
}