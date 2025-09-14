using System.Threading.Tasks;
using System.Security.Claims;
using Arsmate.Core.Entities;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de tokens JWT
    /// </summary>
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Task<User> ValidateRefreshTokenAsync(string refreshToken);
        bool ValidateAccessToken(string token);
        string GetUsernameFromToken(string token);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
