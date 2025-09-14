// DTOs/AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace ArsmateAPI.DTOs
{
    // DTO para Login
    public class LoginDto
    {
        [Required(ErrorMessage = "El email o username es requerido")]
        public string EmailOrUsername { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    // DTO para Registro
    public class RegisterDto
    {
        [Required(ErrorMessage = "El username es requerido")]
        [MinLength(3, ErrorMessage = "El username debe tener al menos 3 caracteres")]
        [MaxLength(50, ErrorMessage = "El username no puede tener más de 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "El username solo puede contener letras, números y guión bajo")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El nombre para mostrar es requerido")]
        [MaxLength(100)]
        public string DisplayName { get; set; }

        public bool IsCreator { get; set; }

        [Required(ErrorMessage = "Debes aceptar los términos y condiciones")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar los términos y condiciones")]
        public bool AcceptTerms { get; set; }
    }

    // DTO para respuesta de autenticación
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public AuthDataDto Data { get; set; }
    }

    public class AuthDataDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserDto User { get; set; }
    }

    // DTO para información de usuario
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public string Bio { get; set; }
        public bool IsCreator { get; set; }
        public bool IsVerified { get; set; }
        public bool EmailConfirmed { get; set; }
        public decimal? SubscriptionPrice { get; set; }
        public decimal? MessagePrice { get; set; }
        public string Currency { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostsCount { get; set; }
    }

    // DTO para refrescar token
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    // DTO para cambio de contraseña
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }

    // DTO para recuperación de contraseña
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }
    }

    // DTO para resetear contraseña
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}