using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Flash.SensitiveWords.API.Middleware;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only enforce API key for internal admin endpoints (sensitivewords CRUD)
        var path = context.Request.Path.Value ?? string.Empty;

        if (path.StartsWith("/sensitivewords", StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith("/sensitivewords/filter", StringComparison.OrdinalIgnoreCase))
        {
            var expected = _configuration["ApiSettings:ApiKey"];
            if (string.IsNullOrWhiteSpace(expected))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API key is not configured");
                return;
            }

            StringValues values;
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out values) || values.Count == 0)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API key");
                return;
            }

            var provided = values.FirstOrDefault();
            if (!string.Equals(provided, expected, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }
        }

        await _next(context);
    }
}
