using Arsmate.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArsmateAPI.Models
{
    [Table("post_views")]
    public class PostView
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("post_id")]
        public string PostId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("ip_address")]
        [MaxLength(45)]
        public string IpAddress { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navegación
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
