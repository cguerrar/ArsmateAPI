using System;
using Arsmate.Core.Enums;

namespace Arsmate.Core.DTOs.Post
{
    /// <summary>
    /// DTO para información de archivo multimedia
    /// </summary>
    public class MediaFileDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string BlurredUrl { get; set; }
        public MediaType Type { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public int? Duration { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int OrderIndex { get; set; }
        public bool IsProcessed { get; set; }
    }
}