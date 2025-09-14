using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;
using Arsmate.Infrastructure.Services;
using Arsmate.API.Hubs;
using ArsmateAPI.Middleware;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Mantener el formato de los nombres de propiedades
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arsmate API",
        Version = "v1",
        Description = "API for Arsmate International - Premium Content Creator Platform",
        Contact = new OpenApiContact
        {
            Name = "Arsmate International",
            Email = "support@arsmate.com",
            Url = new Uri("https://arsmate.com")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure Entity Framework
builder.Services.AddDbContext<ArsmateDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Arsmate.API") // <-- AÑADE ESTA LÍNEA
    ));

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured. Please add Jwt:Secret or Jwt:Key to appsettings.json");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure SignalR authentication
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Política para creadores de contenido
    options.AddPolicy("CreatorOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "IsCreator" && c.Value.Equals("true", StringComparison.OrdinalIgnoreCase))));

    // Política para administradores
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "Role" && c.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase))));

    // Política para usuarios verificados
    options.AddPolicy("VerifiedOnly", policy =>
        policy.RequireClaim("IsVerified", "true"));
});

// ============================================
// CORS CONFIGURATION - CORREGIDO
// ============================================
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",      // Next.js default port
                    "http://localhost:3001",      // Next.js alternate port
                    "https://localhost:3000",     // Next.js with HTTPS
                    "http://127.0.0.1:3000",     // Local IP
                    "http://localhost:5173",      // Vite default port (si usas Vite)
                    "http://localhost:4200"       // Angular default port (si usas Angular)
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()  // IMPORTANTE: Necesario para cookies y autenticación
                .WithExposedHeaders("Content-Disposition"); // Para descargas de archivos
        });
    });
}
else
{
    // En producción, ser más restrictivo
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ArsmatePolicy", policy =>
        {
            policy.WithOrigins(
                    "https://arsmate.com",
                    "https://www.arsmate.com",
                    "https://app.arsmate.com"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition");
        });
    });
}

// Register Services - Dependency Injection
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 102400; // 100KB
});

// Configure Caching
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Límite global
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User?.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Límite específico para creación de contenido
    options.AddPolicy("ContentCreation", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User?.Identity?.Name ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10, // 10 posts por minuto
                Window = TimeSpan.FromMinutes(1)
            }));

    // Límite para uploads de archivos
    options.AddPolicy("FileUpload", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User?.Identity?.Name ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 20, // 20 archivos por minuto
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken: token);
    };
});

// Configure File Upload Limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50MB
});

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
}

// ============================================
// BUILD THE APPLICATION
// ============================================
var app = builder.Build();

// ============================================
// CONFIGURE THE HTTP REQUEST PIPELINE
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arsmate API V1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
        c.DocumentTitle = "Arsmate API Documentation";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
        c.EnableTryItOutByDefault(); // Habilitar "Try it out" por defecto
        c.EnableDeepLinking();
    });
}
else
{
    // En producción, usar HSTS
    app.UseHsts();
}

// ============================================
// MIDDLEWARE PIPELINE - ORDEN IMPORTANTE
// ============================================
app.UseHttpsRedirection();

// CORS debe ir ANTES de Authentication y Authorization
if (app.Environment.IsDevelopment())
{
    app.UseCors(); // Usa la política por defecto configurada arriba
}
else
{
    app.UseCors("ArsmatePolicy"); // Usa la política restrictiva en producción
}

app.UseResponseCaching();
app.UseRateLimiter();

// Custom middleware - Manejo de errores global
app.UseMiddleware<ErrorHandlingMiddleware>();

// Authentication y Authorization DESPUÉS de CORS
app.UseAuthentication();
app.UseAuthorization();



// Map Controllers
app.MapControllers();

// Map SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");

// ============================================
// DATABASE INITIALIZATION
// ============================================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ArsmateDbContext>();

    try
    {
        // Verificar si la base de datos existe, si no, crearla
        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            app.Logger.LogInformation("Applying {MigrationCount} pending migrations...", pendingMigrations.Count());
            dbContext.Database.Migrate();
            app.Logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            // Asegurar que la base de datos existe
            dbContext.Database.EnsureCreated();
            app.Logger.LogInformation("Database is up to date.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database.");
        // No lanzar la excepción para permitir que la aplicación continúe
    }
}

// ============================================
// STARTUP LOGGING
// ============================================
app.Logger.LogInformation("========================================");
app.Logger.LogInformation("🚀 Arsmate API started successfully");
app.Logger.LogInformation("📍 Environment: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("🌐 HTTP URL: http://localhost:5212");
app.Logger.LogInformation("🔒 HTTPS URL: https://localhost:7212");
app.Logger.LogInformation("📚 Swagger UI: https://localhost:7212");
app.Logger.LogInformation("💚 Health Check: https://localhost:7212/api/health");
app.Logger.LogInformation("🔧 CORS Origins Allowed:");
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("   - http://localhost:3000 (Next.js)");
    app.Logger.LogInformation("   - http://localhost:3001");
    app.Logger.LogInformation("   - https://localhost:3000");
    app.Logger.LogInformation("   - http://127.0.0.1:3000");
}
app.Logger.LogInformation("========================================");

app.Run();