using System;
using System.ComponentModel.DataAnnotations;
using Arsmate.Core.Enums;

namespace Arsmate.Core.DTOs.Payment
{
    /// <summary>
    /// DTO para retiros
    /// </summary>
    public class WithdrawalDto
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public WithdrawalStatus Status { get; set; }
        public WithdrawalMethod Method { get; set; }
        public decimal Fee { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string RejectionReason { get; set; }
        public int? EstimatedArrivalDays { get; set; }
    }

    public class CreateWithdrawalDto
    {
        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(20, 999999.99, ErrorMessage = "El monto debe estar entre 20 y 999999.99")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El método de retiro es obligatorio")]
        public WithdrawalMethod Method { get; set; }

        [Required(ErrorMessage = "Los detalles de la cuenta son obligatorios")]
        public string AccountDetails { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string Notes { get; set; }
    }
}