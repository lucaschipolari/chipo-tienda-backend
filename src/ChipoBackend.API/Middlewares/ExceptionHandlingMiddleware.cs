using System.Text.Json;
using ChipoBackend.Domain.Exceptions;
using ValidationException = ChipoBackend.Application.Common.Exceptions.ValidationException;
using NotFoundException = ChipoBackend.Application.Common.Exceptions.NotFoundException;
using ForbiddenException = ChipoBackend.Application.Common.Exceptions.ForbiddenException;
using ConflictException = ChipoBackend.Application.Common.Exceptions.ConflictException;

namespace ChipoBackend.API.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errors) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest,
                ve.Errors.ToDictionary(k => k.Key, v => (object)v.Value)),
            NotFoundException => (StatusCodes.Status404NotFound,
                new Dictionary<string, object> { { "message", exception.Message } }),
            ForbiddenException => (StatusCodes.Status403Forbidden,
                new Dictionary<string, object> { { "message", exception.Message } }),
            ConflictException => (StatusCodes.Status409Conflict,
                new Dictionary<string, object> { { "message", exception.Message } }),
            DomainException => (StatusCodes.Status422UnprocessableEntity,
                new Dictionary<string, object> { { "message", exception.Message } }),
            BusinessRuleException bre => (StatusCodes.Status422UnprocessableEntity,
                new Dictionary<string, object> { { "rule", bre.RuleName }, { "message", bre.Message } }),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized,
                new Dictionary<string, object> { { "message", "No autorizado." } }),
            _ => (StatusCodes.Status500InternalServerError,
                new Dictionary<string, object> { { "message", "Ocurrió un error interno del servidor." } })
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            errors,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
