using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Arsmate.Core.DTOs.User;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;

namespace Arsmate.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ArsmateDbContext _context;
        private readonly IMediaService _mediaService;
        private readonly ILogger<UserService> _logger;

        public UserService(ArsmateDbContext context, IMediaService mediaService, ILogger<UserService> logger)
        {
            _context = context;
            _mediaService = mediaService;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CoverPhotoUrl = user.CoverPhotoUrl,
                Location = user.Location,
                WebsiteUrl = user.WebsiteUrl,
                IsCreator = user.IsCreator,
                IsVerified = user.IsVerified,
                IsActive = user.IsActive,
                SubscriptionPrice = user.SubscriptionPrice,
                Currency = user.Currency,
                ShowPostCount = user.ShowPostCount,
                AllowMessages = user.AllowMessages,
                MessagePrice = user.MessagePrice,
                WelcomeMessage = user.WelcomeMessage,
                WelcomeMessageDiscount = (int?)user.WelcomeMessageDiscount,
                TwitterUsername = user.TwitterUsername,
                InstagramUsername = user.InstagramUsername,
                TikTokUsername = user.TikTokUsername,
                YouTubeUrl = user.YouTubeUrl,
                FollowersCount = user.FollowersCount,
                FollowingCount = user.FollowingCount,
                PostsCount = user.PostsCount,
                TotalLikesReceived = user.TotalLikesReceived,
                ProfileViewsCount = user.ProfileViewsCount,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null) return null;

            return MapToUserDto(user);
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return MapToUserDto(user);
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateProfileDto.DisplayName))
                user.DisplayName = updateProfileDto.DisplayName;

            if (updateProfileDto.Bio != null)
                user.Bio = updateProfileDto.Bio;

            if (updateProfileDto.Location != null)
                user.Location = updateProfileDto.Location;

            if (updateProfileDto.WebsiteUrl != null)
                user.WebsiteUrl = updateProfileDto.WebsiteUrl;

            if (updateProfileDto.SubscriptionPrice.HasValue)
                user.SubscriptionPrice = updateProfileDto.SubscriptionPrice;

            if (updateProfileDto.ShowPostCount.HasValue)
                user.ShowPostCount = updateProfileDto.ShowPostCount.Value;

            if (updateProfileDto.AllowMessages.HasValue)
                user.AllowMessages = updateProfileDto.AllowMessages.Value;

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Profile updated for user: {user.Username}");
            return true;
        }

        public async Task<string> UpdateProfilePictureAsync(Guid userId, IFormFile file)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            // Upload to storage
            var imageUrl = await _mediaService.UploadFileAsync(file, "profile-pictures");

            // Delete old image if exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                await _mediaService.DeleteFileAsync(user.ProfilePictureUrl);
            }

            user.ProfilePictureUrl = imageUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return imageUrl;
        }

        public async Task<string> UpdateCoverPhotoAsync(Guid userId, IFormFile file)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            var imageUrl = await _mediaService.UploadFileAsync(file, "cover-photos");

            if (!string.IsNullOrEmpty(user.CoverPhotoUrl))
            {
                await _mediaService.DeleteFileAsync(user.CoverPhotoUrl);
            }

            user.CoverPhotoUrl = imageUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return imageUrl;
        }

        public async Task<PaginatedResultDto<UserDto>> GetCreatorsAsync(int page = 1, int pageSize = 20)
        {
            var query = _context.Users
                .Where(u => u.IsCreator && u.IsActive && !u.IsSuspended)
                .OrderByDescending(u => u.FollowersCount);

            var totalCount = await query.CountAsync();

            var creators = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => MapToUserDto(u))
                .ToListAsync();

            return new PaginatedResultDto<UserDto>(creators, totalCount, page, pageSize);
        }

        public async Task<PaginatedResultDto<UserDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20)
        {
            var searchQuery = _context.Users
                .Where(u => u.IsActive && !u.IsSuspended &&
                       (u.Username.Contains(query) ||
                        u.DisplayName.Contains(query) ||
                        u.Bio.Contains(query)))
                .OrderByDescending(u => u.FollowersCount);

            var totalCount = await searchQuery.CountAsync();

            var users = await searchQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => MapToUserDto(u))
                .ToListAsync();

            return new PaginatedResultDto<UserDto>(users, totalCount, page, pageSize);
        }

        public async Task<PaginatedResultDto<UserDto>> SearchCreatorsAsync(CreatorSearchDto searchDto)
        {
            var query = _context.Users
                .Where(u => u.IsCreator && u.IsActive && !u.IsSuspended);

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.Query))
            {
                query = query.Where(u =>
                    u.Username.Contains(searchDto.Query) ||
                    u.DisplayName.Contains(searchDto.Query) ||
                    u.Bio.Contains(searchDto.Query));
            }

            if (searchDto.MinPrice.HasValue)
            {
                query = query.Where(u => u.SubscriptionPrice >= searchDto.MinPrice);
            }

            if (searchDto.MaxPrice.HasValue)
            {
                query = query.Where(u => u.SubscriptionPrice <= searchDto.MaxPrice);
            }

            if (searchDto.IsVerified.HasValue)
            {
                query = query.Where(u => u.IsVerified == searchDto.IsVerified);
            }

            // Apply sorting
            query = searchDto.SortBy?.ToLower() switch
            {
                "newest" => query.OrderByDescending(u => u.CreatedAt),
                "price_low" => query.OrderBy(u => u.SubscriptionPrice),
                "price_high" => query.OrderByDescending(u => u.SubscriptionPrice),
                _ => query.OrderByDescending(u => u.FollowersCount)
            };

            var totalCount = await query.CountAsync();

            var creators = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(u => MapToUserDto(u))
                .ToListAsync();

            return new PaginatedResultDto<UserDto>(creators, totalCount, searchDto.Page, searchDto.PageSize);
        }

        public async Task<bool> FollowUserAsync(Guid followerId, Guid followingId)
        {
            // TODO: Implement follow system with a Follow entity
            _logger.LogInformation($"User {followerId} followed user {followingId}");
            return await Task.FromResult(true);
        }

        public async Task<bool> UnfollowUserAsync(Guid followerId, Guid followingId)
        {
            // TODO: Implement unfollow
            _logger.LogInformation($"User {followerId} unfollowed user {followingId}");
            return await Task.FromResult(true);
        }

        public async Task<PaginatedResultDto<FollowDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            // TODO: Implement with Follow entity
            return new PaginatedResultDto<FollowDto>(new List<FollowDto>(), 0, page, pageSize);
        }

        public async Task<PaginatedResultDto<FollowDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            // TODO: Implement with Follow entity
            return new PaginatedResultDto<FollowDto>(new List<FollowDto>(), 0, page, pageSize);
        }

        public async Task<bool> BlockUserAsync(Guid userId, Guid blockedUserId)
        {
            // TODO: Implement block system
            _logger.LogInformation($"User {userId} blocked user {blockedUserId}");
            return await Task.FromResult(true);
        }

        public async Task<bool> UnblockUserAsync(Guid userId, Guid blockedUserId)
        {
            // TODO: Implement unblock
            _logger.LogInformation($"User {userId} unblocked user {blockedUserId}");
            return await Task.FromResult(true);
        }

        public async Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId)
        {
            // TODO: Implement block check
            return await Task.FromResult(false);
        }

        public async Task<bool> DeleteAccountAsync(Guid userId, string password)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid password");
            }

            // Soft delete
            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Account deleted for user: {user.Username}");
            return true;
        }

        public async Task<bool> VerifyCreatorAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Creator verified: {user.Username}");
            return true;
        }

        public async Task<UserDto> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;

            var userId = Guid.Parse(userIdClaim);
            return await GetUserByIdAsync(userId);
        }

        private UserDto MapToUserDto(Core.Entities.User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CoverPhotoUrl = user.CoverPhotoUrl,
                IsCreator = user.IsCreator,
                IsVerified = user.IsVerified,
                IsActive = user.IsActive,
                SubscriptionPrice = user.SubscriptionPrice,
                Currency = user.Currency,
                FollowersCount = user.FollowersCount,
                FollowingCount = user.FollowingCount,
                PostsCount = user.PostsCount,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
