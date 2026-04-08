using HospitalMS.BL.Exceptions;
using HospitalMS.Web.Models;
using System.Net;
using System.Text.Json;

namespace HospitalMS.Web.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    // invoke middleware
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    // handle exception response
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Response.Headers["X-Correlation-ID"].FirstOrDefault() ?? context.TraceIdentifier;
        LogException(exception, correlationId, context);
        var errorResponse = CreateErrorResponse(exception, correlationId, context.Request.Path);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }

    // log exception details
    private void LogException(Exception exception, string correlationId, HttpContext context)
    {
        var logMessage = $"Exception occurred. CorrelationId: {correlationId}, " + $"Path: {context.Request.Path}, " + $"Method: {context.Request.Method}, " + $"User: {context.User?.Identity?.Name ?? "Anonymous"}";
        switch (exception)
        {
            case ValidationException validationEx:
                _logger.LogWarning(validationEx, "{Message}. Validation errors: {@Errors}", logMessage, validationEx.Errors);
                break;
            
            case NotFoundException notFoundEx:
                _logger.LogWarning(notFoundEx, "{Message}. Resource: {ResourceType}, ID: {ResourceId}", logMessage, notFoundEx.ResourceType, notFoundEx.ResourceId);
                break;
            
            case BusinessRuleException businessEx:
                _logger.LogWarning(businessEx, "{Message}. Rule: {RuleName}", logMessage, businessEx.RuleName);
                break;
            
            case ConflictException conflictEx:
                _logger.LogWarning(conflictEx, "{Message}. Conflict type: {ConflictType}", logMessage, conflictEx.ConflictType);
                break;
           
            case UnauthorizedException unauthorizedEx:
                _logger.LogWarning(unauthorizedEx, "{Message}", logMessage);
                break;
            
            case DomainException domainEx:
                _logger.LogError(domainEx, "{Message}. Error code: {ErrorCode}", logMessage, domainEx.ErrorCode);
                break;
            
            default:
                _logger.LogError(exception, "{Message}. Unhandled exception: {ExceptionType}", logMessage, exception.GetType().Name);
                break;
        }
    }

    // create error response
    private ErrorResponse CreateErrorResponse(Exception exception, string correlationId, string path)
    {
        return exception switch
        {
            ValidationException validationEx => ErrorResponse.Create(
                message: "One or more validation errors occurred.",
                errorCode: validationEx.ErrorCode,
                statusCode: (int)HttpStatusCode.BadRequest,
                correlationId: correlationId,
                validationErrors: validationEx.Errors,
                path: path
            ),
            
            NotFoundException notFoundEx => ErrorResponse.Create(
                message: notFoundEx.Message,
                errorCode: notFoundEx.ErrorCode,
                statusCode: (int)HttpStatusCode.NotFound,
                correlationId: correlationId,
                path: path
            ),
            
            BusinessRuleException businessEx => ErrorResponse.Create(
                message: businessEx.Message,
                errorCode: businessEx.ErrorCode,
                statusCode: (int)HttpStatusCode.BadRequest,
                correlationId: correlationId,
                path: path
            ),
           
            ConflictException conflictEx => ErrorResponse.Create(
                message: conflictEx.Message,
                errorCode: conflictEx.ErrorCode,
                statusCode: (int)HttpStatusCode.Conflict,
                correlationId: correlationId,
                path: path
            ),
            
            UnauthorizedException unauthorizedEx => ErrorResponse.Create(
                message: unauthorizedEx.Message,
                errorCode: unauthorizedEx.ErrorCode,
                statusCode: (int)HttpStatusCode.Unauthorized,
                correlationId: correlationId,
                path: path
            ),
           
            DomainException domainEx => ErrorResponse.Create(
                message: domainEx.Message,
                errorCode: domainEx.ErrorCode,
                statusCode: (int)HttpStatusCode.BadRequest,
                correlationId: correlationId,
                path: path
            ),
           
            _ => ErrorResponse.Create(
                message: _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.",
                errorCode: "INTERNAL_SERVER_ERROR",
                statusCode: (int)HttpStatusCode.InternalServerError,
                correlationId: correlationId,
                path: path
            )
        };
    }
}