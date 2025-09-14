using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArsmateAPI.Models
{
    [Table("post_tags")]
    public class PostTag
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("post_id")]
        public string PostId { get; set; }

        [Required]
        [Column("tag")]
        [MaxLength(50)]
        public string Tag { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navegación
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
    }
}