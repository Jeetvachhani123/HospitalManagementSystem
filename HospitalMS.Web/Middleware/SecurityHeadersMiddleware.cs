using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HospitalMS.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // add security headers
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://code.jquery.com; " +
            "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
            "font-src 'self' https://cdnjs.cloudflare.com https://fonts.gstatic.com; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' ws: wss: https://cdnjs.cloudflare.com https://cdn.jsdelivr.net http://localhost:* https://localhost:*; " +
            "frame-ancestors 'self'; " +
            "form-action 'self';");
        await _next(context);
    }
}