using System.Net;
using System.Text.Json;
using SmartBookAPI.DTOs;

namespace SmartBookAPI.Middleware;

/// <summary>
/// Middleware global para capturar y manejar excepciones no controladas
/// </summary>
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
            // Continuar con el siguiente middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            // Capturar cualquier excepción no manejada
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Error no controlado: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ApiResponse
        {
            Success = false,
            Message = "Ha ocurrido un error interno en el servidor"
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "No autorizado";
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "Recurso no encontrado";
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = exception.Message;
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = exception.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // En desarrollo, mostrar el mensaje real del error
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    errorResponse.Message = exception.Message;
                }
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(json);
    }
}
