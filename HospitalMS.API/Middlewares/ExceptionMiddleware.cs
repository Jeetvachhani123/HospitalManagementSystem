using HospitalMS.BL.Common;
using HospitalMS.BL.Exceptions;
using System.Net;
using System.Text.Json;

namespace HospitalMS.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    // invoke middleware pipeline
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    // format error response with proper HTTP status codes
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            ValidationException validation => (HttpStatusCode.BadRequest, validation.Message),
            ConflictException conflict => (HttpStatusCode.Conflict, conflict.Message),
            BusinessRuleException businessRule => ((HttpStatusCode)422, businessRule.Message),
            ConcurrencyException concurrency => (HttpStatusCode.Conflict, concurrency.Message),
            UnauthorizedException unauthorized => (HttpStatusCode.Forbidden, unauthorized.Message),
            _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
        };

        context.Response.StatusCode = (int)statusCode;

        // Only include exception details in development
        var errorDetails = _environment.IsDevelopment() ? exception.Message : null;
        var response = statusCode == HttpStatusCode.InternalServerError
            ? ApiResponse<object>.ErrorResponse(message, errorDetails)
            : ApiResponse<object>.ErrorResponse(message);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}