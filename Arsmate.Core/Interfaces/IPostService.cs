using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Arsmate.Core.DTOs.Post;
using Arsmate.Core.DTOs.Common;
using Microsoft.AspNetCore.Http;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de publicaciones
    /// </summary>
    public interface IPostService
    {
        Task<PostDto> CreatePostAsync(Guid creatorId, CreatePostDto createPostDto);
        Task<PostDto> GetPostByIdAsync(Guid postId, Guid? userId = null);
        Task<bool> UpdatePostAsync(Guid postId, Guid userId, UpdatePostDto updatePostDto);
        Task<bool> DeletePostAsync(Guid postId, Guid userId);
        Task<PaginatedResultDto<PostDto>> GetUserPostsAsync(Guid userId, Guid? viewerId = null, int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<PostDto>> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<PostDto>> GetExplorePostsAsync(Guid? userId = null, int page = 1, int pageSize = 20);
        Task<bool> LikePostAsync(Guid postId, Guid userId);
        Task<bool> UnlikePostAsync(Guid postId, Guid userId);
        Task<PaginatedResultDto<LikeDto>> GetPostLikesAsync(Guid postId, int page = 1, int pageSize = 20);
        Task<CommentDto> AddCommentAsync(Guid postId, Guid userId, CreateCommentDto createCommentDto);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
        Task<PaginatedResultDto<CommentDto>> GetPostCommentsAsync(Guid postId, int page = 1, int pageSize = 20);
        Task<bool> PinPostAsync(Guid postId, Guid userId);
        Task<bool> UnpinPostAsync(Guid postId, Guid userId);
        Task<bool> ArchivePostAsync(Guid postId, Guid userId);
        Task<bool> UnarchivePostAsync(Guid postId, Guid userId);
        Task<bool> PurchasePostAsync(Guid postId, Guid userId, string paymentMethodId);
        Task<List<MediaFileDto>> ProcessMediaFilesAsync(List<IFormFile> files, Guid postId);
        Task<bool> IncrementViewCountAsync(Guid postId);
    }
}