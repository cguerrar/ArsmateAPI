using System;

namespace Arsmate.Core.DTOs.Payment
{
    /// <summary>
    /// DTO para información de billetera
    /// </summary>
    public class WalletDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public decimal PendingBalance { get; set; }
        public string Currency { get; set; }
        public DateTime? LastWithdrawalAt { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public decimal TotalTipsReceived { get; set; }
        public decimal TotalSubscriptionsEarned { get; set; }
        public decimal TotalPPVEarned { get; set; }
        public decimal MinimumWithdrawalAmount { get; set; }
        public bool IsPayoutAccountVerified { get; set; }
        public DateTime? PayoutAccountVerifiedAt { get; set; }
    }
}