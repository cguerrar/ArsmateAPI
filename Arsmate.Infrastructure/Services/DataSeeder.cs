// Services/DataSeeder.cs
using Arsmate.Core.Interfaces;
using Arsmate.Core.Entities;
using Arsmate.Core.Enums;
using Arsmate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Arsmate.Infrastructure.Services
{
    public class DataSeeder : IDataSeeder
    {
        private readonly ArsmateDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            ArsmateDbContext context,
            IConfiguration configuration,
            ILogger<DataSeeder> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Solo ejecutar si la base de datos está vacía
                if (await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("Database already contains data. Skipping seed.");
                    return;
                }

                _logger.LogInformation("Starting database seed...");

                // Crear usuario admin
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@arsmate.com",
                    PasswordHash = HashPassword("Admin123!"), // Usar PasswordHash en lugar de Password
                    DisplayName = "Administrator",
                    Role = UserRole.Admin, // Usar enum en lugar de IsAdmin
                    IsCreator = false,
                    IsVerified = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(adminUser);

                // Crear usuario de prueba (creador)
                var testCreator = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testcreator",
                    Email = "creator@test.com",
                    PasswordHash = HashPassword("Test123!"),
                    DisplayName = "Test Creator",
                    Bio = "Soy un creador de contenido de prueba",
                    Role = UserRole.User, // Role normal
                    IsCreator = true, // Pero es creador
                    IsVerified = true,
                    EmailConfirmed = true,
                    SubscriptionPrice = 9.99m,
                    Currency = "USD",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(testCreator);

                // Crear usuario regular
                var regularUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "user@test.com",
                    PasswordHash = HashPassword("Test123!"),
                    DisplayName = "Test User",
                    Role = UserRole.User,
                    IsCreator = false,
                    IsVerified = false,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(regularUser);

                await _context.SaveChangesAsync();

                // Crear algunos posts de ejemplo
                var samplePost1 = new Post
                {
                    Id = Guid.NewGuid(),
                    AuthorId = testCreator.Id, // Usar AuthorId en lugar de UserId
                    Text = "¡Bienvenidos a mi perfil de Arsmate! Aquí compartiré contenido exclusivo.", // Usar Text en lugar de Content
                    Visibility = PostVisibility.Public, // Usar enum
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Posts.Add(samplePost1);

                var samplePost2 = new Post
                {
                    Id = Guid.NewGuid(),
                    AuthorId = testCreator.Id,
                    Text = "Este contenido es solo para suscriptores. ¡Gracias por su apoyo!",
                    Visibility = PostVisibility.Subscribers, // Usar enum
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Posts.Add(samplePost2);

                // Si tu modelo tiene Tags como entidad separada
                if (_context.GetType().GetProperty("Tags") != null)
                {
                    // Crear tags si existe la tabla
                    var tag1 = new Tag
                    {
                        Id = Guid.NewGuid(),
                        Name = "bienvenida",
                        CreatedAt = DateTime.UtcNow
                    };

                    var tag2 = new Tag
                    {
                        Id = Guid.NewGuid(),
                        Name = "arsmate",
                        CreatedAt = DateTime.UtcNow
                    };

                    // Agregar tags al contexto si existe
                    // _context.Tags.Add(tag1);
                    // _context.Tags.Add(tag2);

                    // Relacionar con posts si tu modelo lo soporta
                    // Esto dependerá de cómo tengas configurada la relación muchos a muchos
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Database seed completed successfully.");
                _logger.LogInformation("Created users: admin (admin@arsmate.com), testcreator (creator@test.com), testuser (user@test.com)");
                _logger.LogInformation("Default password for all users: Test123! (Admin123! for admin)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(
                Encoding.UTF8.GetBytes(password + _configuration["Jwt:Secret"]));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

// Interfaces/IDataSeeder.cs
namespace Arsmate.Core.Interfaces
{
    public interface IDataSeeder
    {
        Task SeedAsync();
    }
}

// Entidades faltantes (si no las tienes)
namespace Arsmate.Core.Entities
{
    // Clase Tag si la necesitas
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relación muchos a muchos con posts
        public virtual ICollection<Post> Posts { get; set; }
    }
}

// Enums que probablemente ya tienes
namespace Arsmate.Core.Enums
{
    public enum UserRole
    {
        User = 0,
        Moderator = 1,
        Admin = 2
    }

    public enum PostVisibility
    {
        Public = 0,
        Followers = 1,
        Subscribers = 2,
        Private = 3
    }
}