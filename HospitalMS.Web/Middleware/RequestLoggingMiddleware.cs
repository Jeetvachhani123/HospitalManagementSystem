namespace HospitalMS.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // invoke middleware
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method,
            ["UserAgent"] = context.Request.Headers["User-Agent"].ToString(),
            ["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        }))
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("HTTP {Method} {Path} started. CorrelationId: {CorrelationId}, IP: {IpAddress}", context.Request.Method, context.Request.Path, correlationId, context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                await _next(context);
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("HTTP {Method} {Path} completed with status {StatusCode} in {Duration}ms. CorrelationId: {CorrelationId}", context.Request.Method, context.Request.Path, context.Response.StatusCode, duration.TotalMilliseconds, correlationId);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "HTTP {Method} {Path} failed after {Duration}ms. CorrelationId: {CorrelationId}", context.Request.Method, context.Request.Path, duration.TotalMilliseconds, correlationId);
                throw;
            }
        }
    }
}