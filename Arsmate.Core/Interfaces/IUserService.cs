using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Arsmate.Core.DTOs.User;
using Arsmate.Core.DTOs.Common;
using System.Security.Claims;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de usuarios
    /// </summary>
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(Guid userId);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto);
        Task<string> UpdateProfilePictureAsync(Guid userId, IFormFile file);
        Task<string> UpdateCoverPhotoAsync(Guid userId, IFormFile file);
        Task<PaginatedResultDto<UserDto>> GetCreatorsAsync(int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<UserDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<UserDto>> SearchCreatorsAsync(CreatorSearchDto searchDto);
        Task<bool> FollowUserAsync(Guid followerId, Guid followingId);
        Task<bool> UnfollowUserAsync(Guid followerId, Guid followingId);
        Task<PaginatedResultDto<FollowDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<FollowDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<bool> BlockUserAsync(Guid userId, Guid blockedUserId);
        Task<bool> UnblockUserAsync(Guid userId, Guid blockedUserId);
        Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId);
        Task<bool> DeleteAccountAsync(Guid userId, string password);
        Task<bool> VerifyCreatorAsync(Guid userId);
        Task<UserDto> GetCurrentUserAsync(ClaimsPrincipal principal);
    }
}
