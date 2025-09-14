// Controllers/TestController.cs - CONTROLADOR DE DIAGNÓSTICO
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Arsmate.Core.Entities;
using Arsmate.Infrastructure.Data;

namespace ArsmateAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ArsmateDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TestController> _logger;

        public TestController(
            ArsmateDbContext context,
            IConfiguration configuration,
            ILogger<TestController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: api/test/check-all
        [HttpGet("check-all")]
        public async Task<IActionResult> CheckAll()
        {
            var results = new Dictionary<string, object>();

            try
            {
                // 1. Verificar conexión a BD
                results["database"] = new
                {
                    canConnect = await _context.Database.CanConnectAsync(),
                    provider = _context.Database.ProviderName,
                    connectionString = _configuration.GetConnectionString("DefaultConnection")?.Replace("Data Source=", "DB File: ")
                };

                // 2. Verificar tablas
                try
                {
                    var userCount = await _context.Users.CountAsync();
                    results["users_table"] = new
                    {
                        exists = true,
                        count = userCount
                    };
                }
                catch (Exception ex)
                {
                    results["users_table"] = new
                    {
                        exists = false,
                        error = ex.Message
                    };
                }

                // 3. Verificar configuración JWT
                var jwtSecret = _configuration["Jwt:Secret"] ?? _configuration["Jwt:Key"];
                results["jwt_config"] = new
                {
                    configured = !string.IsNullOrEmpty(jwtSecret),
                    keyLength = jwtSecret?.Length ?? 0,
                    issuer = _configuration["Jwt:Issuer"],
                    audience = _configuration["Jwt:Audience"]
                };

                // 4. Verificar estructura de la tabla Users
                try
                {
                    var tableInfo = await _context.Database.GetDbConnection()
                        .QueryAsync("PRAGMA table_info(Users)");

                    results["users_columns"] = tableInfo;
                }
                catch
                {
                    results["users_columns"] = "No se pudo obtener información de columnas";
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // POST: api/test/create-user-direct
        [HttpPost("create-user-direct")]
        public async Task<IActionResult> CreateUserDirect()
        {
            try
            {
                _logger.LogInformation("=== INICIANDO CREACIÓN DIRECTA DE USUARIO ===");

                // Crear usuario de prueba
                var testUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = $"test_{DateTime.UtcNow.Ticks}",
                    Email = $"test_{DateTime.UtcNow.Ticks}@test.com",
                    PasswordHash = "TestHash123",
                    DisplayName = "Test User Direct",
                    IsCreator = false,
                    IsVerified = false,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Usuario a crear: {@User}", new
                {
                    testUser.Id,
                    testUser.Username,
                    testUser.Email
                });

                _context.Users.Add(testUser);

                _logger.LogInformation("Usuario agregado al contexto, guardando...");

                var result = await _context.SaveChangesAsync();

                _logger.LogInformation("SaveChanges completado. Filas afectadas: {Rows}", result);

                // Verificar que se guardó
                var savedUser = await _context.Users.FindAsync(testUser.Id);

                if (savedUser != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Usuario creado exitosamente",
                        user = new
                        {
                            savedUser.Id,
                            savedUser.Username,
                            savedUser.Email,
                            savedUser.CreatedAt
                        }
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Usuario no se encontró después de guardar"
                    });
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al crear usuario");

                return StatusCode(500, new
                {
                    error = "Database Update Error",
                    message = dbEx.Message,
                    innerError = dbEx.InnerException?.Message,
                    entries = dbEx.Entries?.Select(e => new
                    {
                        entity = e.Entity.GetType().Name,
                        state = e.State.ToString()
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al crear usuario");

                return StatusCode(500, new
                {
                    error = ex.GetType().Name,
                    message = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: api/test/check-password-hash
        [HttpGet("check-password-hash")]
        public IActionResult CheckPasswordHash()
        {
            try
            {
                var jwtKey = _configuration["Jwt:Secret"] ?? _configuration["Jwt:Key"];
                var testPassword = "Test123!";

                using var sha256 = SHA256.Create();
                var saltedPassword = testPassword + jwtKey;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                var hash = Convert.ToBase64String(hashedBytes);

                return Ok(new
                {
                    testPassword,
                    jwtKeyConfigured = !string.IsNullOrEmpty(jwtKey),
                    jwtKeyLength = jwtKey?.Length ?? 0,
                    resultingHash = hash,
                    hashLength = hash.Length,
                    message = "Use este hash para crear usuarios manualmente en la BD"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/test/test-entity-tracking
        [HttpPost("test-entity-tracking")]
        public async Task<IActionResult> TestEntityTracking()
        {
            try
            {
                // Verificar si hay usuarios con el mismo ID siendo rastreados
                var trackedEntities = _context.ChangeTracker.Entries<User>()
                    .Select(e => new
                    {
                        e.Entity.Id,
                        e.Entity.Username,
                        State = e.State.ToString()
                    })
                    .ToList();

                // Limpiar el tracker
                _context.ChangeTracker.Clear();

                return Ok(new
                {
                    trackedBeforeClear = trackedEntities,
                    trackedAfterClear = _context.ChangeTracker.Entries<User>().Count(),
                    message = "Entity tracker limpiado"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/test/sql-test
        [HttpGet("sql-test")]
        public async Task<IActionResult> SqlTest()
        {
            try
            {
                // Ejecutar SQL directo para verificar la tabla
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Users";

                await _context.Database.OpenConnectionAsync();
                var result = await command.ExecuteScalarAsync();
                await _context.Database.CloseConnectionAsync();

                return Ok(new
                {
                    userCountFromSql = result,
                    message = "SQL directo funcionando"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    message = "Error ejecutando SQL directo"
                });
            }
        }

        // GET: api/test/check-migrations
        [HttpGet("check-migrations")]
        public async Task<IActionResult> CheckMigrations()
        {
            try
            {
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

                return Ok(new
                {
                    pending = pendingMigrations.ToList(),
                    applied = appliedMigrations.ToList(),
                    hasPending = pendingMigrations.Any()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    message = "Error verificando migraciones"
                });
            }
        }
    }

    // Extension para el comando SQL (agregar si no existe)
    public static class DbConnectionExtensions
    {
        public static async Task<List<Dictionary<string, object>>> QueryAsync(this System.Data.Common.DbConnection connection, string sql)
        {
            var results = new List<Dictionary<string, object>>();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(row);
            }

            return results;
        }
    }
}