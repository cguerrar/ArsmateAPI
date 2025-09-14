using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArsmateAPI.Models
{
    [Table("post_media")]
    public class PostMedia
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("post_id")]
        public string PostId { get; set; }

        [Required]
        [Column("filename")]
        [MaxLength(255)]
        public string Filename { get; set; }

        [Required]
        [Column("mime_type")]
        [MaxLength(100)]
        public string MimeType { get; set; }

        [Required]
        [Column("size")]
        public int Size { get; set; }

        [Required]
        [Column("data")]
        public string Data { get; set; } // Base64

        [Column("thumbnail_data")]
        public string ThumbnailData { get; set; } // Base64

        [Column("width")]
        public int? Width { get; set; }

        [Column("height")]
        public int? Height { get; set; }

        [Column("duration")]
        public int? Duration { get; set; } // En segundos para videos/audio

        [Column("position")]
        public int Position { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navegación
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
    }
}