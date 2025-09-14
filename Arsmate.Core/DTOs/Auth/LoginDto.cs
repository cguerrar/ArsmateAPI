using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.DTOs.Auth
{
    /// <summary>
    /// DTO para inicio de sesión
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "El usuario o email es obligatorio")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string DeviceId { get; set; }

        public string DeviceName { get; set; }

        public string IpAddress { get; set; }
    }
}