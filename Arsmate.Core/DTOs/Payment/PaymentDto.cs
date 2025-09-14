using System;
using Arsmate.Core.Enums;

namespace Arsmate.Core.DTOs.Payment
{
    /// <summary>
    /// DTO para información de pago
    /// </summary>
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal? Fee { get; set; }
        public decimal NetAmount { get; set; }
        public string Description { get; set; }
        public string PaymentMethod { get; set; }
        public string CardLast4 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}