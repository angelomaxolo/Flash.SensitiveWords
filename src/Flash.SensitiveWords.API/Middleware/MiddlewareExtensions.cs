using Microsoft.AspNetCore.Builder;
using Flash.SensitiveWords.API.Middleware;

namespace Flash.SensitiveWords.API.Middleware;

/// <summary>
/// Extensions for registering API middleware components.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds request logging middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated application builder.</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
