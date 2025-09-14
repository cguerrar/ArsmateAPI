using Arsmate.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArsmateAPI.Models
{
    [Table("post_comments")]
    public class PostComment
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("post_id")]
        public string PostId { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("parent_id")]
        public string ParentId { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Column("likes_count")]
        public int LikesCount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navegación
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ParentId")]
        public virtual PostComment Parent { get; set; }

        public virtual ICollection<PostComment> Replies { get; set; }
    }
}

