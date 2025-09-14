using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Arsmate.Core.DTOs.User
{
    /// <summary>
    /// DTO para actualizar el perfil del usuario
    /// </summary>
    public class UpdateProfileDto
    {
        [StringLength(100, ErrorMessage = "El nombre para mostrar no puede exceder 100 caracteres")]
        public string DisplayName { get; set; }

        [StringLength(500, ErrorMessage = "La biografía no puede exceder 500 caracteres")]
        public string Bio { get; set; }

        [StringLength(100, ErrorMessage = "La ubicación no puede exceder 100 caracteres")]
        public string Location { get; set; }

        [Url(ErrorMessage = "La URL del sitio web no es válida")]
        [StringLength(200, ErrorMessage = "La URL no puede exceder 200 caracteres")]
        public string WebsiteUrl { get; set; }

        // Configuración de creador
        [Range(0, 999.99, ErrorMessage = "El precio debe estar entre 0 y 999.99")]
        public decimal? SubscriptionPrice { get; set; }

        [StringLength(1000, ErrorMessage = "El mensaje de bienvenida no puede exceder 1000 caracteres")]
        public string WelcomeMessage { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public int? WelcomeMessageDiscount { get; set; }

        public bool? ShowPostCount { get; set; }
        public bool? AllowMessages { get; set; }

        [Range(0, 999.99, ErrorMessage = "El precio del mensaje debe estar entre 0 y 999.99")]
        public decimal? MessagePrice { get; set; }

        // Redes sociales
        [StringLength(50, ErrorMessage = "El usuario de Twitter no puede exceder 50 caracteres")]
        public string TwitterUsername { get; set; }

        [StringLength(50, ErrorMessage = "El usuario de Instagram no puede exceder 50 caracteres")]
        public string InstagramUsername { get; set; }

        [StringLength(50, ErrorMessage = "El usuario de TikTok no puede exceder 50 caracteres")]
        public string TikTokUsername { get; set; }

        [Url(ErrorMessage = "La URL de YouTube no es válida")]
        [StringLength(200, ErrorMessage = "La URL de YouTube no puede exceder 200 caracteres")]
        public string YouTubeUrl { get; set; }

        // Configuración de privacidad
        public bool? IsPublicProfile { get; set; }
        public bool? ShowInSearch { get; set; }
        public bool? ShowInSuggestions { get; set; }
        public bool? AllowComments { get; set; }

        // Notificaciones
        public bool? EmailNotifications { get; set; }
        public bool? PushNotifications { get; set; }
    }
}