using System.Net;
using System.Text.Json;

namespace MediRoute.API.Middleware;

/// <summary>Global exception handler — returns RFC7807-style JSON for unhandled errors.</summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception at {Path}", ctx.Request.Path);
            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var payload = JsonSerializer.Serialize(new
            {
                title = "An unexpected error occurred",
                status = 500,
                detail = ex.Message
            });
            await ctx.Response.WriteAsync(payload);
        }
    }
}
