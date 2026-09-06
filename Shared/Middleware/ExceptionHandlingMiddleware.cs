using System.Text.Json;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Exceptions;

namespace Wedding_Proposal_BE.Shared.Middleware;

/// Centralizes error formatting so controllers stay free of try/catch (Middleware Pipeline Pattern).
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Handled application exception: {ErrorCode}", ex.ErrorCode);
            await WriteResponseAsync(context, ex.StatusCode, ApiResponse.Fail(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("An unexpected error occurred.", "INTERNAL_ERROR"));
        }
    }

    private static Task WriteResponseAsync(HttpContext context, int statusCode, ApiResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return context.Response.WriteAsync(json);
    }
}
