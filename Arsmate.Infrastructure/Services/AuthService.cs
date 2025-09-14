using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Arsmate.Core.DTOs.Auth;
using Arsmate.Core.Entities;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;
using BCrypt.Net;
using Arsmate.Core.DTOs.User;

namespace Arsmate.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ArsmateDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenService _tokenService;

        public AuthService(
            ArsmateDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            ITokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _tokenService = tokenService;
        }

        public async Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation($"Starting registration for username: {registerDto.Username}");

                // Validate username uniqueness
                if (await _context.Users.AnyAsync(u => u.Username.ToLower() == registerDto.Username.ToLower()))
                {
                    throw new InvalidOperationException("Username already exists");
                }

                // Validate email uniqueness
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == registerDto.Email.ToLower()))
                {
                    throw new InvalidOperationException("Email already registered");
                }

                // Create new user with all required fields as non-null
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = registerDto.Username,
                    Email = registerDto.Email.ToLower(),
                    DisplayName = !string.IsNullOrEmpty(registerDto.DisplayName)
                        ? registerDto.DisplayName
                        : registerDto.Username,
                    Bio = "",
                    ProfileImageUrl = "/images/default-avatar.png",
                    ProfilePictureUrl = "/images/default-avatar.png",
                    CoverImageUrl = "/images/default-cover.jpg",
                    CoverPhotoUrl = "/images/default-cover.jpg",
                    IsCreator = registerDto.IsCreator,
                    IsVerified = false,
                    EmailConfirmed = false,
                    EmailConfirmationToken = Guid.NewGuid().ToString(),
                    EmailConfirmationTokenExpires = DateTime.UtcNow.AddDays(7),
                    DateOfBirth = registerDto.DateOfBirth,
                    Location = "",
                    WebsiteUrl = "",

                    // Hash password usando BCrypt
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),

                    // Configuración de creador
                    SubscriptionPrice = registerDto.IsCreator ? 5.00m : null,
                    MessagePrice = registerDto.IsCreator ? 1.00m : null,
                    Currency = "USD",
                    WelcomeMessage = "",
                    WelcomeMessageDiscount = null,

                    // Redes sociales
                    InstagramUsername = "",
                    TwitterUsername = "",
                    TikTokUsername = "",
                    YouTubeUrl = "",

                    // Estadísticas iniciales
                    FollowersCount = 0,
                    FollowingCount = 0,
                    PostsCount = 0,
                    LikesCount = 0,
                    TotalLikesReceived = 0,
                    ProfileViewsCount = 0,

                    // Configuración de privacidad
                    ShowActivityStatus = true,
                    AllowMessages = true,
                    ShowSubscriberCount = true,
                    ShowMediaInProfile = true,
                    ShowPostCount = true,

                    // Notificaciones
                    EmailNotifications = true,
                    PushNotifications = true,
                    PushNotificationToken = "",

                    // Estado de la cuenta
                    IsActive = true,
                    IsSuspended = false,
                    SuspendedUntil = null,
                    SuspensionReason = "",
                    LastLoginAt = null,
                    LastLoginIp = "",

                    // Tokens
                    RefreshToken = "",
                    RefreshTokenExpiryTime = null,
                    PasswordResetToken = "",
                    PasswordResetTokenExpires = null,

                    // Two Factor
                    TwoFactorEnabled = false,
                    TwoFactorSecret = "",

                    // Timestamps
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add user to database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {user.Username} registered successfully with ID {user.Id}");

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Update user with refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                // Create wallet for the user
                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Balance = 0,
                    PendingBalance = 0,
                    TotalEarned = 0,
                    TotalWithdrawn = 0,
                    TotalTipsReceived = 0,
                    TotalSubscriptionsEarned = 0,
                    TotalPPVEarned = 0,
                    MinimumWithdrawalAmount = 20,
                    Currency = "USD",
                    PayPalEmail = "",
                    BankAccountInfo = "", // Agregar este campo
                    StripeAccountId = "", // Posiblemente necesario también
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();

                return new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(
                        _configuration["Jwt:ExpirationInMinutes"] ?? "60")),
                    TokenType = "Bearer",
                    User = MapUserToDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during registration for username: {registerDto.Username}");
                throw;
            }
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by email or username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Email.ToLower() == loginDto.UsernameOrEmail.ToLower() ||
                        u.Username.ToLower() == loginDto.UsernameOrEmail.ToLower());

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Account is deactivated");
                }

                // Check if account is suspended
                if (user.IsSuspended && user.SuspendedUntil > DateTime.UtcNow)
                {
                    throw new UnauthorizedAccessException($"Account is suspended until {user.SuspendedUntil}");
                }

                // Verify password using BCrypt
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = loginDto.IpAddress ?? "";

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Update refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                await _context.SaveChangesAsync();

                return new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(
                        _configuration["Jwt:ExpirationInMinutes"] ?? "60")),
                    TokenType = "Bearer",
                    User = MapUserToDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                throw;
            }
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(
                    _configuration["Jwt:ExpirationInMinutes"] ?? "60")),
                TokenType = "Bearer",
                User = MapUserToDto(user)
            };
        }

        public async Task<bool> RevokeTokenAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user != null)
            {
                user.RefreshToken = "";
                user.RefreshTokenExpiryTime = null;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                // Don't reveal if email exists
                return string.Empty;
            }

            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == resetPasswordDto.Email.ToLower() &&
                    u.PasswordResetToken == resetPasswordDto.Token);

            if (user == null || user.PasswordResetTokenExpires <= DateTime.UtcNow)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            user.PasswordResetToken = "";
            user.PasswordResetTokenExpires = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == email.ToLower() &&
                    u.EmailConfirmationToken == token);

            if (user == null || user.EmailConfirmationTokenExpires <= DateTime.UtcNow)
            {
                return false;
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = "";
            user.EmailConfirmationTokenExpires = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return string.Empty;
            }

            var token = Guid.NewGuid().ToString();
            user.EmailConfirmationToken = token;
            user.EmailConfirmationTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<bool> EnableTwoFactorAsync(Guid userId, EnableTwoFactorDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            // TODO: Implement 2FA logic with authenticator app
            user.TwoFactorEnabled = true;
            user.TwoFactorSecret = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DisableTwoFactorAsync(Guid userId, DisableTwoFactorDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            // TODO: Verify password or 2FA code before disabling
            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = "";
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> VerifyTwoFactorAsync(TwoFactorDto dto)
        {
            // TODO: Implementar verificación real de 2FA cuando se defina el DTO
            // Por ahora, siempre retornamos false para no permitir 2FA
            return false;
        }

        public async Task<bool> ValidateUsernameAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        // Helper method to map User to UserDto
        private UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                IsCreator = user.IsCreator,
                IsVerified = user.IsVerified,
                SubscriptionPrice = user.SubscriptionPrice,
                Currency = user.Currency,
                FollowersCount = user.FollowersCount,
                FollowingCount = user.FollowingCount,
                PostsCount = user.PostsCount
            };
        }
    }
}