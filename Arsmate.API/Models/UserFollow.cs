using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArsmateAPI.Models
{
    [Table("user_follows")]
    public class UserFollow
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("follower_id")]
        public string FollowerId { get; set; }

        [Required]
        [Column("following_id")]
        public string FollowingId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navegación
        [ForeignKey("FollowerId")]
        public virtual User Follower { get; set; }

        [ForeignKey("FollowingId")]
        public virtual User Following { get; set; }
    }
}
