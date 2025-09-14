// ========================================
// Archivo: Arsmate.Core/DTOs/Auth/TwoFactorDto.cs
// ========================================

using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Arsmate.Core.DTOs.Auth
{
    /// <summary>
    /// DTO para verificación de código 2FA
    /// </summary>
    public class TwoFactorDto
    {
        /// <summary>
        /// Código de verificación de 6 dígitos
        /// </summary>
        [Required(ErrorMessage = "The verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The verification code must contain only numbers")]
        public string Code { get; set; }

        /// <summary>
        /// Token temporal para identificar la sesión de autenticación
        /// </summary>
        [Required(ErrorMessage = "The authentication token is required")]
        public string Token { get; set; }

        /// <summary>
        /// Indica si el dispositivo debe ser recordado para futuros logins
        /// </summary>
        public bool RememberDevice { get; set; }

        /// <summary>
        /// Identificador único del dispositivo (opcional)
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Nombre del dispositivo (opcional) - ej: "iPhone de Juan"
        /// </summary>
        [StringLength(100, ErrorMessage = "Device name cannot exceed 100 characters")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Tipo de dispositivo (opcional) - ej: "mobile", "desktop", "tablet"
        /// </summary>
        [StringLength(50, ErrorMessage = "Device type cannot exceed 50 characters")]
        public string DeviceType { get; set; }

        /// <summary>
        /// Sistema operativo del dispositivo (opcional)
        /// </summary>
        [StringLength(50, ErrorMessage = "Operating system cannot exceed 50 characters")]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Navegador usado (opcional)
        /// </summary>
        [StringLength(50, ErrorMessage = "Browser cannot exceed 50 characters")]
        public string Browser { get; set; }

        /// <summary>
        /// Dirección IP del dispositivo (se obtiene del servidor)
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Ubicación aproximada basada en IP (opcional)
        /// </summary>
        public string Location { get; set; }
    }

    /// <summary>
    /// DTO para habilitar autenticación de dos factores
    /// </summary>
    public class EnableTwoFactorDto
    {
        /// <summary>
        /// Contraseña actual del usuario para confirmar la acción
        /// </summary>
        [Required(ErrorMessage = "Password is required to enable two-factor authentication")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Tipo de 2FA a habilitar
        /// </summary>
        [Required(ErrorMessage = "Two-factor method is required")]
        public TwoFactorMethod Method { get; set; }

        /// <summary>
        /// Número de teléfono para SMS (requerido si Method es SMS)
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Email alternativo para códigos (opcional)
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string BackupEmail { get; set; }

        /// <summary>
        /// Indica si desea generar códigos de respaldo
        /// </summary>
        public bool GenerateBackupCodes { get; set; } = true;

        /// <summary>
        /// Número de códigos de respaldo a generar
        /// </summary>
        [Range(5, 20, ErrorMessage = "Backup codes count must be between 5 and 20")]
        public int BackupCodesCount { get; set; } = 10;
    }

    /// <summary>
    /// DTO para deshabilitar autenticación de dos factores
    /// </summary>
    public class DisableTwoFactorDto
    {
        /// <summary>
        /// Contraseña actual del usuario para confirmar la acción
        /// </summary>
        [Required(ErrorMessage = "Password is required to disable two-factor authentication")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Código de verificación actual para confirmar
        /// </summary>
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The verification code must contain only numbers")]
        public string Code { get; set; }

        /// <summary>
        /// Razón para deshabilitar 2FA (opcional)
        /// </summary>
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; }

        /// <summary>
        /// Indica si se debe revocar todos los dispositivos confiables
        /// </summary>
        public bool RevokeAllTrustedDevices { get; set; } = true;
    }

    /// <summary>
    /// DTO para configuración inicial de 2FA con authenticator app
    /// </summary>
    public class SetupTwoFactorDto
    {
        /// <summary>
        /// Clave secreta compartida para el authenticator
        /// </summary>
        public string SharedKey { get; set; }

        /// <summary>
        /// URI formateado para código QR
        /// </summary>
        public string AuthenticatorUri { get; set; }

        /// <summary>
        /// Códigos de recuperación generados
        /// </summary>
        public List<string> RecoveryCodes { get; set; }

        /// <summary>
        /// Instrucciones para el usuario
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// URL del código QR como imagen (opcional)
        /// </summary>
        public string QrCodeImageUrl { get; set; }

        /// <summary>
        /// Clave manual para entrada sin QR
        /// </summary>
        public string ManualEntryKey { get; set; }

        /// <summary>
        /// Tiempo de expiración de la configuración
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO para verificar código durante la configuración
    /// </summary>
    public class VerifySetupTwoFactorDto
    {
        /// <summary>
        /// Código de verificación del authenticator
        /// </summary>
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The verification code must contain only numbers")]
        public string Code { get; set; }

        /// <summary>
        /// Token de sesión de configuración
        /// </summary>
        [Required(ErrorMessage = "Setup token is required")]
        public string SetupToken { get; set; }

        /// <summary>
        /// Indica si el usuario ha guardado los códigos de recuperación
        /// </summary>
        [Required(ErrorMessage = "You must confirm that you have saved the recovery codes")]
        public bool RecoveryCodesSaved { get; set; }
    }

    /// <summary>
    /// DTO para regenerar códigos de recuperación
    /// </summary>
    public class RegenerateRecoveryCodesDto
    {
        /// <summary>
        /// Contraseña actual para confirmar
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Código 2FA actual para verificación
        /// </summary>
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be exactly 6 digits")]
        public string VerificationCode { get; set; }

        /// <summary>
        /// Número de códigos a generar
        /// </summary>
        [Range(5, 20, ErrorMessage = "Number of codes must be between 5 and 20")]
        public int NumberOfCodes { get; set; } = 10;
    }

    /// <summary>
    /// DTO para respuesta de códigos de recuperación
    /// </summary>
    public class RecoveryCodesResponseDto
    {
        /// <summary>
        /// Lista de códigos de recuperación
        /// </summary>
        public List<string> RecoveryCodes { get; set; }

        /// <summary>
        /// Fecha de generación
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Número de códigos no utilizados restantes
        /// </summary>
        public int UnusedCodesCount { get; set; }

        /// <summary>
        /// Advertencia para el usuario
        /// </summary>
        public string Warning { get; set; } = "Store these codes in a safe place. Each code can only be used once.";
    }

    /// <summary>
    /// DTO para login con código de recuperación
    /// </summary>
    public class RecoveryCodeLoginDto
    {
        /// <summary>
        /// Nombre de usuario o email
        /// </summary>
        [Required(ErrorMessage = "Username or email is required")]
        public string UsernameOrEmail { get; set; }

        /// <summary>
        /// Contraseña
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Código de recuperación
        /// </summary>
        [Required(ErrorMessage = "Recovery code is required")]
        [StringLength(10, MinimumLength = 8, ErrorMessage = "Invalid recovery code format")]
        [RegularExpression(@"^[A-Z0-9]{8,10}$", ErrorMessage = "Recovery code must contain only uppercase letters and numbers")]
        public string RecoveryCode { get; set; }

        /// <summary>
        /// Recordar este dispositivo
        /// </summary>
        public bool RememberDevice { get; set; }
    }

    /// <summary>
    /// DTO para gestión de dispositivos confiables
    /// </summary>
    public class TrustedDeviceDto
    {
        /// <summary>
        /// ID del dispositivo
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del dispositivo
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Tipo de dispositivo
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Sistema operativo
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Navegador
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// Última dirección IP conocida
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Última ubicación conocida
        /// </summary>
        public string LastLocation { get; set; }

        /// <summary>
        /// Fecha cuando se confió en el dispositivo
        /// </summary>
        public DateTime TrustedAt { get; set; }

        /// <summary>
        /// Último uso del dispositivo
        /// </summary>
        public DateTime LastUsedAt { get; set; }

        /// <summary>
        /// Fecha de expiración de la confianza
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Indica si es el dispositivo actual
        /// </summary>
        public bool IsCurrentDevice { get; set; }
    }

    /// <summary>
    /// DTO para revocar un dispositivo confiable
    /// </summary>
    public class RevokeTrustedDeviceDto
    {
        /// <summary>
        /// ID del dispositivo a revocar
        /// </summary>
        [Required(ErrorMessage = "Device ID is required")]
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Contraseña para confirmar la acción
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Razón de la revocación (opcional)
        /// </summary>
        [StringLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
        public string Reason { get; set; }
    }

    /// <summary>
    /// DTO para cambiar método de 2FA
    /// </summary>
    public class ChangeTwoFactorMethodDto
    {
        /// <summary>
        /// Método actual de 2FA
        /// </summary>
        [Required(ErrorMessage = "Current method is required")]
        public TwoFactorMethod CurrentMethod { get; set; }

        /// <summary>
        /// Nuevo método de 2FA
        /// </summary>
        [Required(ErrorMessage = "New method is required")]
        public TwoFactorMethod NewMethod { get; set; }

        /// <summary>
        /// Contraseña para confirmar
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Código de verificación del método actual
        /// </summary>
        [Required(ErrorMessage = "Current verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be exactly 6 digits")]
        public string CurrentVerificationCode { get; set; }

        /// <summary>
        /// Número de teléfono (si el nuevo método es SMS)
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Email (si el nuevo método es Email)
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
    }

    /// <summary>
    /// DTO para enviar código 2FA por SMS o Email
    /// </summary>
    public class SendTwoFactorCodeDto
    {
        /// <summary>
        /// Token de sesión temporal
        /// </summary>
        [Required(ErrorMessage = "Session token is required")]
        public string SessionToken { get; set; }

        /// <summary>
        /// Método de envío
        /// </summary>
        [Required(ErrorMessage = "Delivery method is required")]
        public TwoFactorDeliveryMethod DeliveryMethod { get; set; }

        /// <summary>
        /// Número de teléfono alternativo (opcional)
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number")]
        public string AlternatePhoneNumber { get; set; }

        /// <summary>
        /// Email alternativo (opcional)
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string AlternateEmail { get; set; }
    }

    /// <summary>
    /// Métodos disponibles de autenticación de dos factores
    /// </summary>
    public enum TwoFactorMethod
    {
        /// <summary>
        /// Aplicación de autenticación (Google Authenticator, Authy, etc.)
        /// </summary>
        Authenticator = 0,

        /// <summary>
        /// SMS al teléfono registrado
        /// </summary>
        SMS = 1,

        /// <summary>
        /// Email al correo registrado
        /// </summary>
        Email = 2,

        /// <summary>
        /// Llave de seguridad física (FIDO2/WebAuthn)
        /// </summary>
        SecurityKey = 3,

        /// <summary>
        /// Autenticación biométrica
        /// </summary>
        Biometric = 4
    }

    /// <summary>
    /// Métodos de entrega para códigos 2FA
    /// </summary>
    public enum TwoFactorDeliveryMethod
    {
        /// <summary>
        /// SMS al número principal
        /// </summary>
        PrimarySMS = 0,

        /// <summary>
        /// SMS a número alternativo
        /// </summary>
        AlternateSMS = 1,

        /// <summary>
        /// Email principal
        /// </summary>
        PrimaryEmail = 2,

        /// <summary>
        /// Email alternativo
        /// </summary>
        AlternateEmail = 3,

        /// <summary>
        /// Llamada de voz
        /// </summary>
        VoiceCall = 4
    }

    /// <summary>
    /// DTO para historial de autenticación 2FA
    /// </summary>
    public class TwoFactorHistoryDto
    {
        /// <summary>
        /// ID del evento
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tipo de evento
        /// </summary>
        public TwoFactorEventType EventType { get; set; }

        /// <summary>
        /// Método usado
        /// </summary>
        public TwoFactorMethod Method { get; set; }

        /// <summary>
        /// Resultado del intento
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Dirección IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Ubicación
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Información del dispositivo
        /// </summary>
        public string DeviceInfo { get; set; }

        /// <summary>
        /// Fecha y hora del evento
        /// </summary>
        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// Mensaje de error (si falló)
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Tipos de eventos de 2FA para auditoría
    /// </summary>
    public enum TwoFactorEventType
    {
        /// <summary>
        /// 2FA habilitado
        /// </summary>
        Enabled = 0,

        /// <summary>
        /// 2FA deshabilitado
        /// </summary>
        Disabled = 1,

        /// <summary>
        /// Verificación exitosa
        /// </summary>
        VerificationSuccess = 2,

        /// <summary>
        /// Verificación fallida
        /// </summary>
        VerificationFailed = 3,

        /// <summary>
        /// Código de recuperación usado
        /// </summary>
        RecoveryCodeUsed = 4,

        /// <summary>
        /// Códigos de recuperación regenerados
        /// </summary>
        RecoveryCodesRegenerated = 5,

        /// <summary>
        /// Método cambiado
        /// </summary>
        MethodChanged = 6,

        /// <summary>
        /// Dispositivo confiable añadido
        /// </summary>
        TrustedDeviceAdded = 7,

        /// <summary>
        /// Dispositivo confiable revocado
        /// </summary>
        TrustedDeviceRevoked = 8,

        /// <summary>
        /// Código enviado por SMS/Email
        /// </summary>
        CodeSent = 9
    }
}
