using Application_Layer_temp.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace YourAPP_Web.CustomMiddlewares;

public class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next.Invoke(httpContext);
            await HandleNotFoundEndPointAsync(httpContext); // handles empty 404s (no route matched)
        }
        catch (ValidationException ex)
        {
            // FluentValidation pipeline threw — group errors by field
            logger.LogWarning("Validation failed at {Method} {Path}: {ValidationErrors}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                ex.Message);

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            await WriteProblemAsync(httpContext,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Failed",
                detail: "One or more validation errors occurred.",
                extensions: new Dictionary<string, object?>
                {
                    ["errors"]    = errors,
                    ["traceId"]   = httpContext.TraceIdentifier,
                    ["timestamp"] = DateTime.UtcNow
                });
        }
        catch (Exception ex)
        {
            // log everything — full stack trace goes to logs
            logger.LogError(ex,
                "Exception [{ExceptionType}] at {Method} {Path} — {Message}",
                ex.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                ex.Message);

            // map exception type → HTTP status code
            var statusCode = ex switch
            {
                NotFoundException           => StatusCodes.Status404NotFound,
                ConflictException           => StatusCodes.Status409Conflict,
                ForbiddenException          => StatusCodes.Status403Forbidden,
                BadRequestException         => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                InvalidOperationException   => StatusCodes.Status400BadRequest,
                ServiceUnavailableException => StatusCodes.Status503ServiceUnavailable,
                _                           => StatusCodes.Status500InternalServerError
            };

            // known exceptions → show message | unknown → hide internals from client
            var detail = ex switch
            {
                NotFoundException           => ex.Message,
                ConflictException           => ex.Message,
                ForbiddenException          => ex.Message,
                BadRequestException         => ex.Message,
                UnauthorizedAccessException => ex.Message,
                InvalidOperationException   => ex.Message,
                ServiceUnavailableException => ex.Message,
                _                           => "An unexpected error occurred. Please try again later."
            };

            var title = ex switch
            {
                NotFoundException           => "Resource Not Found",
                ConflictException           => "Conflict",
                ForbiddenException          => "Forbidden",
                BadRequestException         => "Bad Request",
                UnauthorizedAccessException => "Unauthorized",
                InvalidOperationException   => "Invalid Operation",
                ServiceUnavailableException => "Service Unavailable",
                _                           => "Internal Server Error"
            };

            await WriteProblemAsync(httpContext,
                statusCode: statusCode,
                title: title,
                detail: detail,
                extensions: new Dictionary<string, object?>
                {
                    ["exceptionType"] = ex.GetType().Name,
                    ["traceId"]       = httpContext.TraceIdentifier,
                    ["timestamp"]     = DateTime.UtcNow,
                    // only expose stackTrace in Development
                    ["stackTrace"]    = IsDevEnvironment() ? ex.StackTrace : null
                });
        }
    }

    // ── Handles routes that don't exist (no exception thrown, just empty 404) ──────

    private static async Task HandleNotFoundEndPointAsync(HttpContext httpContext)
    {
        if (httpContext.Response.StatusCode == StatusCodes.Status404NotFound
            && !httpContext.Response.HasStarted)
        {
            await WriteProblemAsync(httpContext,
                statusCode: StatusCodes.Status404NotFound,
                title: "Endpoint Not Found",
                detail: $"The endpoint '{httpContext.Request.Method} {httpContext.Request.Path}' does not exist.",
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"]   = httpContext.TraceIdentifier,
                    ["timestamp"] = DateTime.UtcNow
                });
        }
    }

    // ── Single writer — every response goes through here ─────────────────────────

    private static async Task WriteProblemAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        Dictionary<string, object?> extensions)
    {
        var problem = new ProblemDetails
        {
            Title    = title,
            Detail   = detail,
            Status   = statusCode,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}" // e.g. "POST /api/products/999"
        };

        foreach (var (key, value) in extensions)
            problem.Extensions[key] = value;

        httpContext.Response.StatusCode  = statusCode;
        httpContext.Response.ContentType = "application/problem+json"; // RFC 7807 MIME type

        await httpContext.Response.WriteAsJsonAsync(problem);
    }

    // ── Only show stackTrace in Development ───────────────────────────────────────

    private static bool IsDevEnvironment()
        => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
}
