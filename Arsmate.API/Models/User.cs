using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArsmateAPI.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("username")]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [Column("password")]
        public string Password { get; set; }

        [Column("display_name")]
        [MaxLength(100)]
        public string DisplayName { get; set; }

        [Column("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [Column("cover_image_url")]
        public string CoverImageUrl { get; set; }

        [Column("bio")]
        [MaxLength(500)]
        public string Bio { get; set; }

        [Column("is_creator")]
        public bool IsCreator { get; set; }

        [Column("is_verified")]
        public bool IsVerified { get; set; }

        [Column("is_admin")]
        public bool IsAdmin { get; set; }

        [Column("subscription_price")]
        public decimal? SubscriptionPrice { get; set; }

        [Column("message_price")]
        public decimal? MessagePrice { get; set; }

        [Column("currency")]
        [MaxLength(3)]
        public string Currency { get; set; }

        [Column("followers_count")]
        public int FollowersCount { get; set; }

        [Column("following_count")]
        public int FollowingCount { get; set; }

        [Column("posts_count")]
        public int PostsCount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navegación
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<PostLike> Likes { get; set; }
        public virtual ICollection<PostBookmark> Bookmarks { get; set; }
        public virtual ICollection<PostComment> Comments { get; set; }
        public virtual ICollection<UserFollow> Followers { get; set; }
        public virtual ICollection<UserFollow> Following { get; set; }
        public virtual ICollection<UserSubscription> Subscribers { get; set; }
        public virtual ICollection<UserSubscription> Subscriptions { get; set; }
    }
}
