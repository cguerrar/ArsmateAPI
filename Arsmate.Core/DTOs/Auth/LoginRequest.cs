using System.ComponentModel.DataAnnotations;

namespace Arsmate.Api.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El email o nombre de usuario es requerido")]
        public string EmailOrUsername { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
