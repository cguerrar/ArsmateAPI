using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArsmateAPI.Models
{
    [Table("user_subscriptions")]
    public class UserSubscription
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("subscriber_id")]
        public string SubscriberId { get; set; }

        [Required]
        [Column("creator_id")]
        public string CreatorId { get; set; }

        [Required]
        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } // active, cancelled, expired

        [Column("price")]
        public decimal Price { get; set; }

        [Column("currency")]
        [MaxLength(3)]
        public string Currency { get; set; }

        [Column("started_at")]
        public DateTime StartedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navegación
        [ForeignKey("SubscriberId")]
        public virtual User Subscriber { get; set; }

        [ForeignKey("CreatorId")]
        public virtual User Creator { get; set; }
    }
}