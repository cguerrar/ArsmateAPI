using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.DTOs.Auth
{
    /// <summary>
    /// DTO para solicitud de recuperación de contraseña
    /// </summary>
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }
    }
}