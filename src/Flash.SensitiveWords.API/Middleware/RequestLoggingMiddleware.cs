using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Flash.SensitiveWords.API.Middleware;

/// <summary>
/// Middleware that logs request metadata, completion, and unhandled exceptions for each incoming HTTP request.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware with the current <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? context.TraceIdentifier;
        var operationId = activity?.RootId ?? string.Empty;
        var spanId = activity?.SpanId.ToString() ?? string.Empty;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = traceId,
            ["OperationId"] = operationId,
            ["SpanId"] = spanId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method
        }))
        {
            _logger.LogInformation("Starting HTTP request {Method} {Path} TraceId={TraceId} OperationId={OperationId}.",
                context.Request.Method,
                context.Request.Path,
                traceId,
                operationId);

            try
            {
                await _next(context);
                stopwatch.Stop();
                _logger.LogInformation("Completed HTTP request {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms.",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unhandled exception for HTTP request {Method} {Path} TraceId={TraceId} OperationId={OperationId} after {ElapsedMilliseconds}ms.",
                    context.Request.Method,
                    context.Request.Path,
                    traceId,
                    operationId,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
