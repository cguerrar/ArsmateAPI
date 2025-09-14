using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Arsmate.Core.DTOs.Auth;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Interfaces;

namespace Arsmate.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponseDto<TokenResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid data provided"));
                }

                var result = await _authService.RegisterAsync(registerDto);

                _logger.LogInformation($"New user registered: {registerDto.Username}");

                return Ok(ApiResponseDto<TokenResponseDto>.SuccessResponse(
                    result,
                    "Registration successful. Please check your email to confirm your account."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred during registration"));
            }
        }

        /// <summary>
        /// Login with username/email and password
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDto<TokenResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid data provided"));
                }

                // Add IP address for security logging
                loginDto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var result = await _authService.LoginAsync(loginDto);

                return Ok(ApiResponseDto<TokenResponseDto>.SuccessResponse(
                    result,
                    "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseDto<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred during login"));
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponseDto<TokenResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto);

                return Ok(ApiResponseDto<TokenResponseDto>.SuccessResponse(result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseDto<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred while refreshing token"));
            }
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var username = User.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    await _authService.RevokeTokenAsync(username);
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "Logged out successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred during logout"));
            }
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var token = await _authService.GeneratePasswordResetTokenAsync(forgotPasswordDto.Email);

                // Always return success to prevent email enumeration
                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "If the email exists, a password reset link has been sent."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred processing your request"));
            }
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(resetPasswordDto);

                if (!result)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid or expired token"));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "Password reset successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred resetting password"));
            }
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

                if (!result)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Failed to change password"));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "Password changed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred changing password"));
            }
        }

        /// <summary>
        /// Validate if username is available
        /// </summary>
        [HttpGet("validate-username/{username}")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> ValidateUsername(string username)
        {
            var isAvailable = await _authService.ValidateUsernameAsync(username);
            return Ok(new { available = isAvailable });
        }

        /// <summary>
        /// Validate if email is available
        /// </summary>
        [HttpGet("validate-email/{email}")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> ValidateEmail(string email)
        {
            var isAvailable = await _authService.ValidateEmailAsync(email);
            return Ok(new { available = isAvailable });
        }

        /// <summary>
        /// Confirm email address
        /// </summary>
        [HttpPost("confirm-email")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                var result = await _authService.ConfirmEmailAsync(email, token);

                if (!result)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid confirmation token"));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "Email confirmed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred confirming email"));
            }
        }

        /// <summary>
        /// Enable two-factor authentication
        /// </summary>
        [HttpPost("enable-2fa")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var result = await _authService.EnableTwoFactorAsync(userId, dto);

                if (!result)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Failed to enable two-factor authentication"));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    null,
                    "Two-factor authentication enabled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA");
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                    "An error occurred enabling two-factor authentication"));
            }
        }





    }
}
