using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.DTOs.Auth
{
    /// <summary>
    /// DTO para refrescar tokens
    /// </summary>
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "El access token es obligatorio")]
        public string AccessToken { get; set; }

        [Required(ErrorMessage = "El refresh token es obligatorio")]
        public string RefreshToken { get; set; }
    }
}