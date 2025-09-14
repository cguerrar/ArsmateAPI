// ====================================
// MediaService.cs
// ====================================
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Arsmate.Core.Entities;
using Arsmate.Core.Enums;
using Arsmate.Core.Interfaces;
using Arsmate.Core.DTOs;
using Arsmate.Infrastructure.Data;
using Arsmate.Core.DTOs.Post;

namespace Arsmate.Infrastructure.Services
{
    /// <summary>
    /// Servicio simplificado para gestión de archivos multimedia
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly ArsmateDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MediaService> _logger;
        private readonly string _basePath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".mov", ".avi", ".webm", ".mkv" };

        public MediaService(
            ArsmateDbContext context,
            IConfiguration configuration,
            ILogger<MediaService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _basePath = configuration["Media:StoragePath"] ?? "wwwroot/uploads";

            var maxFileSizeConfig = configuration["Media:MaxFileSize"];
            _maxFileSize = !string.IsNullOrEmpty(maxFileSizeConfig)
                ? long.Parse(maxFileSizeConfig)
                : 104857600; // 100MB default
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty");

                if (file.Length > _maxFileSize)
                    throw new ArgumentException($"File exceeds maximum size");

                var uploadPath = Path.Combine(_basePath, folder);
                Directory.CreateDirectory(uploadPath);

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                return false;
            }
        }

        public async Task<MediaFileDto> ProcessMediaFileAsync(IFormFile file, Guid? postId = null, Guid? messageId = null)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var mediaType = GetMediaType(extension);

            var uploadDate = DateTime.UtcNow;
            var relativePath = Path.Combine(
                uploadDate.Year.ToString(),
                uploadDate.Month.ToString("D2")
            );

            var fileUrl = await UploadFileAsync(file, relativePath);

            var mediaFile = new MediaFile
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                MessageId = messageId,
                FileUrl = fileUrl,
                FileName = file.FileName,
                FileSize = file.Length,
                MimeType = file.ContentType,
                Type = mediaType,
                Order = 0,
                CreatedAt = uploadDate,
                UpdatedAt = uploadDate
            };

            _context.MediaFiles.Add(mediaFile);
            await _context.SaveChangesAsync();

            return new MediaFileDto
            {
                Id = mediaFile.Id,
                FileUrl = mediaFile.FileUrl,
                FileName = mediaFile.FileName,
                FileSize = mediaFile.FileSize,
                MimeType = mediaFile.MimeType,
                Type = mediaFile.Type
            };
        }

        public async Task<List<MediaFileDto>> ProcessMultipleMediaFilesAsync(List<IFormFile> files, Guid? postId = null, Guid? messageId = null)
        {
            var results = new List<MediaFileDto>();
            var order = 0;

            foreach (var file in files)
            {
                var mediaDto = await ProcessMediaFileAsync(file, postId, messageId);
                results.Add(mediaDto);
                order++;
            }

            return results;
        }

        public async Task<bool> ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedImageExtensions.Contains(extension) ||
                   _allowedVideoExtensions.Contains(extension);
        }

        public async Task<string> GenerateThumbnailAsync(string filePath)
        {
            // Implementación simplificada
            await Task.Delay(10);
            return filePath; // En producción usar ImageSharp o FFMpeg
        }

        public async Task<string> GenerateBlurredImageAsync(string filePath)
        {
            // Implementación simplificada
            await Task.Delay(10);
            return filePath; // En producción usar ImageSharp
        }

        public async Task<string> OptimizeImageAsync(string filePath, int maxWidth, int maxHeight)
        {
            // Implementación simplificada
            await Task.Delay(10);
            return filePath; // En producción usar ImageSharp
        }

        public async Task<string> ConvertVideoAsync(string inputPath, string outputFormat)
        {
            // Implementación simplificada
            await Task.Delay(10);
            return inputPath; // En producción usar FFMpeg
        }

        public async Task<Dictionary<string, object>> GetMediaMetadataAsync(string filePath)
        {
            // Implementación simplificada
            await Task.Delay(10);
            return new Dictionary<string, object>
            {
                ["duration"] = 0,
                ["width"] = 0,
                ["height"] = 0
            };
        }

        public async Task<string> GetFileUrlAsync(string fileName, string folder)
        {
            await Task.Delay(1);
            return $"/{folder}/{fileName}";
        }

        public async Task<MediaFileDto> UploadFileAsync(IFormFile file, Guid userId, object uploadDto)
        {
            // Implementación simplificada para cumplir con la interfaz
            return await ProcessMediaFileAsync(file);
        }

        public async Task<bool> DeleteFileAsync(Guid mediaFileId, Guid userId)
        {
            var mediaFile = await _context.MediaFiles.FindAsync(mediaFileId);
            if (mediaFile == null)
                return false;

            await DeleteFileAsync(mediaFile.FileUrl);
            _context.MediaFiles.Remove(mediaFile);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MediaFileDto> GetMediaFileAsync(Guid mediaFileId, Guid userId)
        {
            var mediaFile = await _context.MediaFiles.FindAsync(mediaFileId);
            if (mediaFile == null)
                return null;

            return new MediaFileDto
            {
                Id = mediaFile.Id,
                FileUrl = mediaFile.FileUrl,
                FileName = mediaFile.FileName,
                FileSize = mediaFile.FileSize,
                MimeType = mediaFile.MimeType,
                Type = mediaFile.Type
            };
        }

        public async Task<IEnumerable<MediaFileDto>> GetPostMediaAsync(Guid postId, Guid userId)
        {
            var mediaFiles = await _context.MediaFiles
                .Where(m => m.PostId == postId)
                .OrderBy(m => m.Order)
                .ToListAsync();

            return mediaFiles.Select(m => new MediaFileDto
            {
                Id = m.Id,
                FileUrl = m.FileUrl,
                FileName = m.FileName,
                FileSize = m.FileSize,
                MimeType = m.MimeType,
                Type = m.Type
            });
        }

        public async Task<bool> UpdateMediaOrderAsync(Guid mediaFileId, int newOrder, Guid userId)
        {
            var mediaFile = await _context.MediaFiles.FindAsync(mediaFileId);
            if (mediaFile == null)
                return false;

            mediaFile.Order = newOrder;
            mediaFile.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<long> GetUserStorageUsageAsync(Guid userId)
        {
            var posts = await _context.Posts
                .Where(p => p.CreatorId == userId)
                .Select(p => p.Id)
                .ToListAsync();

            return await _context.MediaFiles
                .Where(m => m.PostId != null && posts.Contains(m.PostId.Value))
                .SumAsync(m => m.FileSize);
        }

        public async Task CleanupUnusedFilesAsync()
        {
            // Implementación para limpiar archivos no utilizados
            await Task.Delay(10);
        }

        private MediaType GetMediaType(string extension)
        {
            if (_allowedImageExtensions.Contains(extension))
                return MediaType.Image;
            if (_allowedVideoExtensions.Contains(extension))
                return MediaType.Video;
            return MediaType.Audio;
        }

        Task<string> IMediaService.UploadFileAsync(IFormFile file, string containerName)
        {
            throw new NotImplementedException();
        }

        Task<MediaFile> IMediaService.ProcessMediaFileAsync(IFormFile file, Guid? postId, Guid? messageId)
        {
            throw new NotImplementedException();
        }

        Task<List<MediaFile>> IMediaService.ProcessMultipleMediaFilesAsync(List<IFormFile> files, Guid? postId, Guid? messageId)
        {
            throw new NotImplementedException();
        }

        Task<bool> IMediaService.DeleteFileAsync(string fileUrl)
        {
            throw new NotImplementedException();
        }

        Task<string> IMediaService.GenerateThumbnailAsync(string videoUrl)
        {
            throw new NotImplementedException();
        }

        Task<string> IMediaService.GenerateBlurredImageAsync(string imageUrl)
        {
            throw new NotImplementedException();
        }

        Task<bool> IMediaService.ValidateFileAsync(IFormFile file)
        {
            throw new NotImplementedException();
        }

        Task<string> IMediaService.GetFileUrlAsync(string fileName, string containerName)
        {
            throw new NotImplementedException();
        }

        Task<long> IMediaService.GetUserStorageUsageAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        Task<bool> IMediaService.CleanupUnusedFilesAsync()
        {
            throw new NotImplementedException();
        }

        Task<string> IMediaService.OptimizeImageAsync(string imageUrl, int maxWidth, int quality)
        {
            throw new NotImplementedException();
        }

        Task<string> IMediaService.ConvertVideoAsync(string videoUrl, string format)
        {
            throw new NotImplementedException();
        }

        Task<Dictionary<string, object>> IMediaService.GetMediaMetadataAsync(string fileUrl)
        {
            throw new NotImplementedException();
        }
    }
}