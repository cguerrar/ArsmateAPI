using Arsmate.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArsmateAPI.Models
{
    [Table("posts")]
    public class Post
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; }

        [Required]
        [Column("content")]
        [MaxLength(5000)]
        public string Content { get; set; }

        [Required]
        [Column("visibility")]
        [MaxLength(20)]
        public string Visibility { get; set; } // public, followers, subscribers

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("is_paid")]
        public bool IsPaid { get; set; }

        [Column("likes_count")]
        public int LikesCount { get; set; }

        [Column("comments_count")]
        public int CommentsCount { get; set; }

        [Column("shares_count")]
        public int SharesCount { get; set; }

        [Column("views_count")]
        public int ViewsCount { get; set; }

        [Column("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }

        [Column("published_at")]
        public DateTime? PublishedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navegación
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<PostMedia> Media { get; set; }
        public virtual ICollection<PostTag> Tags { get; set; }
        public virtual ICollection<PostLike> Likes { get; set; }
        public virtual ICollection<PostBookmark> Bookmarks { get; set; }
        public virtual ICollection<PostComment> Comments { get; set; }
        public virtual ICollection<PostView> Views { get; set; }
    }
}

