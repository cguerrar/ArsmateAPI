// Controllers/AuthController.cs - VERSIÓN FINAL
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Arsmate.Core.Entities;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;
using ArsmateAPI.DTOs;

namespace ArsmateAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ArsmateDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokenService;

        public AuthController(
            ArsmateDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            ITokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _tokenService = tokenService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                _logger.LogInformation("Intento de login para: {EmailOrUsername}", dto?.EmailOrUsername);

                // Validar entrada
                if (dto == null || string.IsNullOrEmpty(dto.EmailOrUsername))
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Datos de login inválidos"
                    });
                }

                // Buscar usuario
                var normalizedInput = dto.EmailOrUsername.ToLower().Trim();
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Username.ToLower() == normalizedInput ||
                        u.Email.ToLower() == normalizedInput);

                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {EmailOrUsername}", dto.EmailOrUsername);

                    // Log para debug
                    var userCount = await _context.Users.CountAsync();
                    _logger.LogInformation("Total de usuarios en BD: {Count}", userCount);

                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    });
                }

                _logger.LogInformation("Usuario encontrado: {Username}", user.Username);

                // Verificar contraseña
                if (!VerifyPassword(dto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Username}", user.Username);
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    });
                }

                _logger.LogInformation("Login exitoso para usuario: {Username}", user.Username);

                // Generar tokens usando ITokenService
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Calcular fecha de expiración
                var expiresInMinutes = Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"] ?? "60");
                var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Data = new AuthDataDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = expiresAt,
                        TokenType = "Bearer",
                        User = MapUserToDto(user)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login para: {EmailOrUsername}", dto?.EmailOrUsername);
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Error al iniciar sesión. Por favor, intente nuevamente."
                });
            }
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                _logger.LogInformation("Iniciando registro para usuario: {Username}", dto?.Username);

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", errors)
                    });
                }

                // Verificar si el username ya existe
                var usernameExists = await _context.Users
                    .AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower());

                if (usernameExists)
                {
                    _logger.LogWarning("Username ya existe: {Username}", dto.Username);
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "El nombre de usuario ya está en uso"
                    });
                }

                // Verificar si el email ya existe
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

                if (emailExists)
                {
                    _logger.LogWarning("Email ya existe: {Email}", dto.Email);
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "El email ya está registrado"
                    });
                }

                // Crear el usuario
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = dto.Username.ToLower(),
                    Email = dto.Email.ToLower(),
                    PasswordHash = HashPassword(dto.Password),
                    DisplayName = dto.DisplayName,
                    IsCreator = dto.IsCreator,
                    IsVerified = false,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    // Inicializar contadores
                    FollowersCount = 0,
                    FollowingCount = 0,
                    PostsCount = 0
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario registrado exitosamente: {UserId} - {Username}", user.Id, user.Username);

                // Generar tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                var expiresInMinutes = Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"] ?? "60");
                var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Registro exitoso",
                    Data = new AuthDataDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = expiresAt,
                        TokenType = "Bearer",
                        User = MapUserToDto(user)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro para usuario: {Username}", dto?.Username);
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Error al registrar el usuario. Por favor, intente nuevamente."
                });
            }
        }

        // POST: api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto?.RefreshToken))
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Refresh token requerido"
                    });
                }

                // Validar refresh token y obtener usuario
                var user = await _tokenService.ValidateRefreshTokenAsync(dto.RefreshToken);

                if (user == null)
                {
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Token inválido o expirado"
                    });
                }

                // Generar nuevos tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                var expiresInMinutes = Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"] ?? "60");
                var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Token actualizado",
                    Data = new AuthDataDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = expiresAt,
                        TokenType = "Bearer",
                        User = MapUserToDto(user)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refrescando token");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Error al actualizar el token"
                });
            }
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                // Aquí podrías invalidar el refresh token en la BD si lo estás guardando
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                _logger.LogInformation("Usuario {Username} cerró sesión", username);

                return Ok(new { success = true, message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en logout");
                return StatusCode(500, new { success = false, message = "Error al cerrar sesión" });
            }
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out var userId))
                {
                    return Unauthorized();
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "Usuario no encontrado" });
                }

                return Ok(new
                {
                    success = true,
                    data = MapUserToDto(user)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario actual");
                return StatusCode(500, new { success = false, message = "Error al obtener información del usuario" });
            }
        }

        // GET: api/auth/validate-username/{username}
        [HttpGet("validate-username/{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateUsername(string username)
        {
            try
            {
                var exists = await _context.Users
                    .AnyAsync(u => u.Username.ToLower() == username.ToLower());

                return Ok(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando username: {Username}", username);
                return Ok(new { available = false });
            }
        }

        // GET: api/auth/validate-email/{email}
        [HttpGet("validate-email/{email}")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateEmail(string email)
        {
            try
            {
                var exists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());

                return Ok(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando email: {Email}", email);
                return Ok(new { available = false });
            }
        }

        // GET: api/auth/test-db
        [HttpGet("test-db")]
        [AllowAnonymous]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        HasPassword = !string.IsNullOrEmpty(u.PasswordHash),
                        u.CreatedAt
                    })
                    .Take(5)
                    .ToListAsync();

                var jwtKey = _configuration["Jwt:Secret"] ?? _configuration["Jwt:Key"];

                return Ok(new
                {
                    success = true,
                    userCount,
                    users,
                    databaseConnection = "OK",
                    jwtConfigured = !string.IsNullOrEmpty(jwtKey),
                    jwtKeyLength = jwtKey?.Length ?? 0,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test-db");
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // Métodos privados auxiliares
        private string HashPassword(string password)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Secret"] ?? _configuration["Jwt:Key"];

                if (string.IsNullOrEmpty(jwtKey))
                {
                    _logger.LogError("JWT Secret/Key no configurado en appsettings.json");
                    throw new InvalidOperationException("JWT key not configured");
                }

                using var sha256 = SHA256.Create();
                var saltedPassword = password + jwtKey;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al hashear contraseña");
                throw;
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                if (string.IsNullOrEmpty(hash))
                {
                    _logger.LogError("Hash de contraseña es null o vacío");
                    return false;
                }

                var passwordHash = HashPassword(password);
                var result = passwordHash == hash;

                // Log para debug (remover en producción)
                if (!result)
                {
                    _logger.LogDebug("Hash esperado: {Expected}", hash.Substring(0, Math.Min(10, hash.Length)));
                    _logger.LogDebug("Hash calculado: {Calculated}", passwordHash.Substring(0, Math.Min(10, passwordHash.Length)));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar contraseña");
                return false;
            }
        }

        private UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ProfileImageUrl = user.ProfileImageUrl,
                CoverImageUrl = user.CoverImageUrl,
                Bio = user.Bio,
                IsCreator = user.IsCreator,
                IsVerified = user.IsVerified,
                EmailConfirmed = user.EmailConfirmed,
                SubscriptionPrice = user.SubscriptionPrice,
                MessagePrice = user.MessagePrice,
                Currency = user.Currency ?? "USD",
                FollowersCount = user.FollowersCount,
                FollowingCount = user.FollowingCount,
                PostsCount = user.PostsCount
            };
        }
    }
}