using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Arsmate.Core.DTOs.User;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Interfaces;

namespace Arsmate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), 200)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var profile = await _userService.GetUserProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(ApiResponseDto<UserProfileDto>.ErrorResponse(
                        "User not found"));
                }

                return Ok(ApiResponseDto<UserProfileDto>.SuccessResponse(profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, ApiResponseDto<UserProfileDto>.ErrorResponse(
                    "An error occurred retrieving profile"));
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        [HttpGet("{username}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponseDto<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<UserDto>), 404)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    return NotFound(ApiResponseDto<UserDto>.ErrorResponse(
                        "User not found"));
                }

                return Ok(ApiResponseDto<UserDto>.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user: {username}");
                return StatusCode(500, ApiResponseDto<UserDto>.ErrorResponse(
                    "An error occurred retrieving user"));
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var result = await _userService.UpdateProfileAsync(userId, updateProfileDto);

                if (!result)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Failed to update profile"));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "Profile updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred updating profile"));
            }
        }

        /// <summary>
        /// Upload profile picture
        /// </summary>
        [HttpPost("profile-picture")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "No file uploaded"));
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid file type. Only JPEG, PNG and GIF are allowed"));
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "File size cannot exceed 5MB"));
                }

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var imageUrl = await _userService.UpdateProfilePictureAsync(userId, file);

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    new { profilePictureUrl = imageUrl },
                    "Profile picture updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred uploading profile picture"));
            }
        }

        /// <summary>
        /// Get list of creators
        /// </summary>
        [HttpGet("creators")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponseDto<PaginatedResultDto<UserDto>>), 200)]
        public async Task<IActionResult> GetCreators([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var creators = await _userService.GetCreatorsAsync(page, pageSize);
                return Ok(ApiResponseDto<PaginatedResultDto<UserDto>>.SuccessResponse(creators));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting creators");
                return StatusCode(500, ApiResponseDto<PaginatedResultDto<UserDto>>.ErrorResponse(
                    "An error occurred retrieving creators"));
            }
        }

        /// <summary>
        /// Search users
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponseDto<PaginatedResultDto<UserDto>>), 200)]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int page = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(ApiResponseDto<PaginatedResultDto<UserDto>>.ErrorResponse(
                        "Search query is required"));
                }

                var results = await _userService.SearchUsersAsync(query, page);
                return Ok(ApiResponseDto<PaginatedResultDto<UserDto>>.SuccessResponse(results));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching users: {query}");
                return StatusCode(500, ApiResponseDto<PaginatedResultDto<UserDto>>.ErrorResponse(
                    "An error occurred searching users"));
            }
        }

        /// <summary>
        /// Follow a user
        /// </summary>
        [HttpPost("{userId}/follow")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> FollowUser(Guid userId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

                if (currentUserId == userId)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "You cannot follow yourself"));
                }

                var result = await _userService.FollowUserAsync(currentUserId, userId);

                if (!result)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Failed to follow user"));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "User followed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error following user: {userId}");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred following user"));
            }
        }

        /// <summary>
        /// Unfollow a user
        /// </summary>
        [HttpDelete("{userId}/unfollow")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> UnfollowUser(Guid userId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var result = await _userService.UnfollowUserAsync(currentUserId, userId);

                if (!result)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Failed to unfollow user"));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "User unfollowed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unfollowing user: {userId}");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred unfollowing user"));
            }
        }
    }
}