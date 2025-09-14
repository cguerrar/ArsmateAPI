using System;
using Arsmate.Core.DTOs.User;

namespace Arsmate.Core.DTOs.Auth
{
    /// <summary>
    /// DTO para respuesta de autenticación con tokens
    /// </summary>
    public class TokenResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserDto User { get; set; }
    }
}