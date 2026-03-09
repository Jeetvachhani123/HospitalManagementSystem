using System.Collections.Concurrent;

namespace HospitalMS.Web.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RequestInfo> _requestCounts = new();
    private static DateTime _lastCleanup = DateTime.MinValue;
    private static readonly object _cleanupLock = new();
    private const int MaxRequestsPerMinute = 100;
    private const int MaxLoginAttemptsPerMinute = 5;
    private const int BlockDurationMinutes = 15;
    private const int CleanupIntervalMinutes = 10;
    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // invoke middleware
    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.ToString().ToLower();
        CleanupOldEntries();
        var requestInfo = _requestCounts.GetOrAdd(ipAddress, _ => new RequestInfo());
        if (requestInfo.IsBlocked && requestInfo.BlockedUntil > DateTime.UtcNow)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}. Blocked until {BlockedUntil}", ipAddress, requestInfo.BlockedUntil);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = "900";
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }
        if (DateTime.UtcNow - requestInfo.WindowStart > TimeSpan.FromMinutes(1))
        {
            requestInfo.RequestCount = 0;
            requestInfo.LoginAttempts = 0;
            requestInfo.WindowStart = DateTime.UtcNow;
        }
        requestInfo.RequestCount++;
        if (path.Contains("/account/login") && context.Request.Method == "POST")
        {
            requestInfo.LoginAttempts++;
            if (requestInfo.LoginAttempts > MaxLoginAttemptsPerMinute)
            {
                requestInfo.IsBlocked = true;
                requestInfo.BlockedUntil = DateTime.UtcNow.AddMinutes(BlockDurationMinutes);
                _logger.LogWarning("Too many login attempts from IP: {IpAddress}. Blocking for {Minutes} minutes", ipAddress, BlockDurationMinutes);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many login attempts. Please try again later.");
                return;
            }
        }
        if (requestInfo.RequestCount > MaxRequestsPerMinute)
        {
            requestInfo.IsBlocked = true;
            requestInfo.BlockedUntil = DateTime.UtcNow.AddMinutes(BlockDurationMinutes);
            _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}. Blocking for {Minutes} minutes", ipAddress, BlockDurationMinutes);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }
        await _next(context);
    }

    // clean expired entries
    private void CleanupOldEntries()
    {
        if (DateTime.UtcNow - _lastCleanup < TimeSpan.FromMinutes(CleanupIntervalMinutes))
        {
            return;
        }
        bool lockTaken = false;
        try
        {
            Monitor.TryEnter(_cleanupLock, ref lockTaken);
            if (lockTaken)
            {
                if (DateTime.UtcNow - _lastCleanup < TimeSpan.FromMinutes(CleanupIntervalMinutes))
                {
                    return;
                }
                var cutoff = DateTime.UtcNow.AddHours(-1);
                var keysToRemove = _requestCounts
                    .Where(kvp => kvp.Value.WindowStart < cutoff && !kvp.Value.IsBlocked)
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (var key in keysToRemove)
                {
                    _requestCounts.TryRemove(key, out _);
                }
                _lastCleanup = DateTime.UtcNow;
            }
        }
        finally
        {
            if (lockTaken)
            {
                Monitor.Exit(_cleanupLock);
            }
        }
    }

    // request tracking info
    private class RequestInfo
    {
        public int RequestCount { get; set; }

        public int LoginAttempts { get; set; }

        public DateTime WindowStart { get; set; } = DateTime.UtcNow;

        public bool IsBlocked { get; set; }

        public DateTime BlockedUntil { get; set; }
    }
}