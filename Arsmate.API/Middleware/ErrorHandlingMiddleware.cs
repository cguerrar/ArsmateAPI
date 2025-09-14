// Middleware/ErrorHandlingMiddleware.cs
using System.Net;
using System.Text.Json;

namespace ArsmateAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case NotFoundException notFound:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFound.Message;
                    response.Details = notFound.Details;
                    break;

                case ValidationException validation:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = validation.Message;
                    response.Errors = validation.Errors;
                    break;

                case UnauthorizedException unauthorized:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = unauthorized.Message;
                    break;

                case ForbiddenException forbidden:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Message = forbidden.Message;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Ha ocurrido un error en el servidor";

                    // Solo incluir detalles en desarrollo
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        response.Details = exception.ToString();
                    }
                    break;
            }

            response.Success = false;
            response.StatusCode = context.Response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    // Clase de respuesta de error
    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string Details { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }

    // Excepciones personalizadas
    public class NotFoundException : Exception
    {
        public string Details { get; set; }

        public NotFoundException(string message, string details = null) : base(message)
        {
            Details = details;
        }
    }

    public class ValidationException : Exception
    {
        public Dictionary<string, string[]> Errors { get; set; }

        public ValidationException(string message, Dictionary<string, string[]> errors = null) : base(message)
        {
            Errors = errors;
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "No autorizado") : base(message)
        {
        }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Acceso prohibido") : base(message)
        {
        }
    }
}