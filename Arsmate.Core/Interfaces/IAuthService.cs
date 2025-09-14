using System;
using System.Threading.Tasks;
using Arsmate.Core.DTOs.Auth;
using Arsmate.Core.Entities;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de autenticación
    /// </summary>
    public interface IAuthService
    {
        Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<TokenResponseDto> LoginAsync(LoginDto loginDto);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> RevokeTokenAsync(string username);
        Task<bool> ValidateUsernameAsync(string username);
        Task<bool> ValidateEmailAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<string> GenerateEmailConfirmationTokenAsync(string email);
        Task<bool> EnableTwoFactorAsync(Guid userId, EnableTwoFactorDto dto);
        Task<bool> DisableTwoFactorAsync(Guid userId, DisableTwoFactorDto dto);
        Task<bool> VerifyTwoFactorAsync(TwoFactorDto dto);
    }
}
