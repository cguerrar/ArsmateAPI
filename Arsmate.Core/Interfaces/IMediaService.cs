using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Arsmate.Core.Entities;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de medios
    /// </summary>
    public interface IMediaService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task<MediaFile> ProcessMediaFileAsync(IFormFile file, Guid? postId = null, Guid? messageId = null);
        Task<List<MediaFile>> ProcessMultipleMediaFilesAsync(List<IFormFile> files, Guid? postId = null, Guid? messageId = null);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<string> GenerateThumbnailAsync(string videoUrl);
        Task<string> GenerateBlurredImageAsync(string imageUrl);
        Task<bool> ValidateFileAsync(IFormFile file);
        Task<string> GetFileUrlAsync(string fileName, string containerName);
        Task<long> GetUserStorageUsageAsync(Guid userId);
        Task<bool> CleanupUnusedFilesAsync();
        Task<string> OptimizeImageAsync(string imageUrl, int maxWidth = 1920, int quality = 85);
        Task<string> ConvertVideoAsync(string videoUrl, string format = "mp4");
        Task<Dictionary<string, object>> GetMediaMetadataAsync(string fileUrl);
    }
}