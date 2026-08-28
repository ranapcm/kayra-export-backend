using Microsoft.AspNetCore.Mvc;

namespace KayraExport.Auth.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            var statusCode = exception switch
            {
                UnauthorizedAccessException =>
                    StatusCodes.Status401Unauthorized,

                ArgumentException =>
                    StatusCodes.Status400BadRequest,

                InvalidOperationException =>
                    StatusCodes.Status409Conflict,

                KeyNotFoundException =>
                    StatusCodes.Status404NotFound,

                _ =>
                    StatusCodes.Status500InternalServerError
            };

            _logger.LogError(
                exception,
                "Auth request failed with status {StatusCode}",
                statusCode);

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Detail = statusCode ==
                    StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : exception.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType =
                "application/problem+json";

            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            404 => "Not Found",
            409 => "Conflict",
            _ => "Internal Server Error"
        };
    }
}