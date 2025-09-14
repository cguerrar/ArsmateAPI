using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Arsmate.Core.DTOs.Post;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Entities;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;

namespace Arsmate.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly ArsmateDbContext _context;
        private readonly IMediaService _mediaService;
        private readonly ILogger<PostService> _logger;

        public PostService(ArsmateDbContext context, IMediaService mediaService, ILogger<PostService> logger)
        {
            _context = context;
            _mediaService = mediaService;
            _logger = logger;
        }

        public async Task<PostDto> CreatePostAsync(Guid creatorId, CreatePostDto createPostDto)
        {
            var post = new Post
            {
                Id = Guid.NewGuid(),
                CreatorId = creatorId,
                Caption = createPostDto.Caption,
                Type = createPostDto.Type,
                Visibility = createPostDto.Visibility,
                Price = createPostDto.Price,
                CommentsEnabled = createPostDto.CommentsEnabled,
                ScheduledAt = createPostDto.ScheduledAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Process media files if any
            if (createPostDto.MediaFiles != null && createPostDto.MediaFiles.Any())
            {
                var mediaFiles = await _mediaService.ProcessMultipleMediaFilesAsync(
                    createPostDto.MediaFiles, post.Id, null);
                post.MediaFiles = mediaFiles;
            }

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Post created: {post.Id} by user: {creatorId}");

            return await GetPostByIdAsync(post.Id, creatorId);
        }

        public async Task<PostDto> GetPostByIdAsync(Guid postId, Guid? userId = null)
        {
            var post = await _context.Posts
                .Include(p => p.Creator)
                .Include(p => p.MediaFiles)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null) return null;

            var dto = new PostDto
            {
                Id = post.Id,
                CreatorId = post.CreatorId,
                Caption = post.Caption,
                Type = post.Type,
                Visibility = post.Visibility,
                Price = post.Price,
                Currency = "USD",
                IsArchived = post.IsArchived,
                CommentsEnabled = post.CommentsEnabled,
                IsPinned = post.IsPinned,
                LikesCount = post.LikesCount,
                CommentsCount = post.CommentsCount,
                ViewsCount = post.ViewsCount,
                SharesCount = post.SharesCount,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                MediaFiles = post.MediaFiles.Select(m => new MediaFileDto
                {
                    Id = m.Id,
                    FileUrl = m.FileUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    BlurredUrl = m.BlurredUrl,
                    Type = m.Type,
                    FileName = m.FileName,
                    MimeType = m.MimeType,
                    FileSize = m.FileSize,
                    Duration = m.Duration,
                    Width = m.Width,
                    Height = m.Height,
                    OrderIndex = m.OrderIndex,
                    IsProcessed = m.IsProcessed
                }).ToList()
            };

            // Check user-specific data
            if (userId.HasValue)
            {
                dto.IsLiked = await _context.Likes
                    .AnyAsync(l => l.PostId == postId && l.UserId == userId.Value);

                dto.IsPurchased = await _context.PostPurchases
                    .AnyAsync(pp => pp.PostId == postId && pp.UserId == userId.Value);

                dto.CanView = post.CreatorId == userId.Value || dto.IsPurchased || post.Price == null;
            }

            return dto;
        }

        public async Task<bool> UpdatePostAsync(Guid postId, Guid userId, UpdatePostDto updatePostDto)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.CreatorId != userId) return false;

            if (updatePostDto.Caption != null)
                post.Caption = updatePostDto.Caption;

            if (updatePostDto.Visibility.HasValue)
                post.Visibility = updatePostDto.Visibility.Value;

            if (updatePostDto.Price.HasValue)
                post.Price = updatePostDto.Price;

            if (updatePostDto.CommentsEnabled.HasValue)
                post.CommentsEnabled = updatePostDto.CommentsEnabled.Value;

            if (updatePostDto.IsArchived.HasValue)
                post.IsArchived = updatePostDto.IsArchived.Value;

            if (updatePostDto.IsPinned.HasValue)
                post.IsPinned = updatePostDto.IsPinned.Value;

            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Post updated: {postId}");
            return true;
        }

        public async Task<bool> DeletePostAsync(Guid postId, Guid userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.CreatorId != userId) return false;

            post.IsDeleted = true;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Post deleted: {postId}");
            return true;
        }

        public async Task<PaginatedResultDto<PostDto>> GetUserPostsAsync(Guid userId, Guid? viewerId = null, int page = 1, int pageSize = 20)
        {
            var query = _context.Posts
                .Include(p => p.Creator)
                .Include(p => p.MediaFiles)
                .Where(p => p.CreatorId == userId && !p.IsDeleted && !p.IsArchived)
                .OrderByDescending(p => p.IsPinned)
                .ThenByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postDtos = new List<PostDto>();
            foreach (var post in posts)
            {
                postDtos.Add(await GetPostByIdAsync(post.Id, viewerId));
            }

            return new PaginatedResultDto<PostDto>(postDtos, totalCount, page, pageSize);
        }

        public async Task<PaginatedResultDto<PostDto>> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            // Get posts from subscribed creators
            var subscribedCreatorIds = await _context.Subscriptions
                .Where(s => s.SubscriberId == userId && s.IsActive)
                .Select(s => s.CreatorId)
                .ToListAsync();

            var query = _context.Posts
                .Include(p => p.Creator)
                .Include(p => p.MediaFiles)
                .Where(p => subscribedCreatorIds.Contains(p.CreatorId) && !p.IsDeleted && !p.IsArchived)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postDtos = new List<PostDto>();
            foreach (var post in posts)
            {
                postDtos.Add(await GetPostByIdAsync(post.Id, userId));
            }

            return new PaginatedResultDto<PostDto>(postDtos, totalCount, page, pageSize);
        }

        public async Task<PaginatedResultDto<PostDto>> GetExplorePostsAsync(Guid? userId = null, int page = 1, int pageSize = 20)
        {
            var query = _context.Posts
                .Include(p => p.Creator)
                .Include(p => p.MediaFiles)
                .Where(p => p.Visibility == Core.Enums.PostVisibility.Public &&
                       !p.IsDeleted && !p.IsArchived)
                .OrderByDescending(p => p.ViewsCount)
                .ThenByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postDtos = new List<PostDto>();
            foreach (var post in posts)
            {
                postDtos.Add(await GetPostByIdAsync(post.Id, userId));
            }

            return new PaginatedResultDto<PostDto>(postDtos, totalCount, page, pageSize);
        }

        public async Task<bool> LikePostAsync(Guid postId, Guid userId)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null) return false;

            var like = new Like
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Likes.AddAsync(like);

            // Update like count
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.LikesCount++;
                post.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikePostAsync(Guid postId, Guid userId)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (like == null) return false;

            _context.Likes.Remove(like);

            // Update like count
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.LikesCount = Math.Max(0, post.LikesCount - 1);
                post.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedResultDto<LikeDto>> GetPostLikesAsync(Guid postId, int page = 1, int pageSize = 20)
        {
            var query = _context.Likes
                .Include(l => l.User)
                .Where(l => l.PostId == postId)
                .OrderByDescending(l => l.CreatedAt);

            var totalCount = await query.CountAsync();

            var likes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LikeDto
                {
                    Id = l.Id,
                    PostId = l.PostId,
                    UserId = l.UserId,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResultDto<LikeDto>(likes, totalCount, page, pageSize);
        }

        public async Task<CommentDto> AddCommentAsync(Guid postId, Guid userId, CreateCommentDto createCommentDto)
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                Content = createCommentDto.Content,
                ParentCommentId = createCommentDto.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Comments.AddAsync(comment);

            // Update comment count
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.CommentsCount++;
                post.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new CommentDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserId = comment.UserId,
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.UserId != userId) return false;

            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;

            // Update comment count
            var post = await _context.Posts.FindAsync(comment.PostId);
            if (post != null)
            {
                post.CommentsCount = Math.Max(0, post.CommentsCount - 1);
                post.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedResultDto<CommentDto>> GetPostCommentsAsync(Guid postId, int page = 1, int pageSize = 20)
        {
            var query = _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId && !c.IsDeleted && !c.IsHidden)
                .OrderByDescending(c => c.IsPinned)
                .ThenByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync();

            var comments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    Content = c.Content,
                    ParentCommentId = c.ParentCommentId,
                    LikesCount = c.LikesCount,
                    RepliesCount = c.RepliesCount,
                    IsEdited = c.IsEdited,
                    EditedAt = c.EditedAt,
                    IsPinned = c.IsPinned,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResultDto<CommentDto>(comments, totalCount, page, pageSize);
        }

        public async Task<bool> PinPostAsync(Guid postId, Guid userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.CreatorId != userId) return false;

            post.IsPinned = true;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpinPostAsync(Guid postId, Guid userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.CreatorId != userId) return false;

            post.IsPinned = false;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchivePostAsync(Guid postId, Guid userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.CreatorId != userId) return false;

            post.IsArchived = true;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnarchivePostAsync(Guid postId, Guid userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.CreatorId != userId) return false;

            post.IsArchived = false;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PurchasePostAsync(Guid postId, Guid userId, string paymentMethodId)
        {
            // TODO: Implement payment processing
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.Price == null) return false;

            var purchase = new PostPurchase
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                PricePaid = post.Price.Value,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.PostPurchases.AddAsync(purchase);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Post {postId} purchased by user {userId}");
            return true;
        }

        public async Task<List<MediaFileDto>> ProcessMediaFilesAsync(List<IFormFile> files, Guid postId)
        {
            var mediaFiles = await _mediaService.ProcessMultipleMediaFilesAsync(files, postId, null);

            return mediaFiles.Select(m => new MediaFileDto
            {
                Id = m.Id,
                FileUrl = m.FileUrl,
                ThumbnailUrl = m.ThumbnailUrl,
                BlurredUrl = m.BlurredUrl,
                Type = m.Type,
                FileName = m.FileName,
                MimeType = m.MimeType,
                FileSize = m.FileSize,
                Duration = m.Duration,
                Width = m.Width,
                Height = m.Height,
                OrderIndex = m.OrderIndex,
                IsProcessed = m.IsProcessed
            }).ToList();
        }

        public async Task<bool> IncrementViewCountAsync(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            post.ViewsCount++;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
