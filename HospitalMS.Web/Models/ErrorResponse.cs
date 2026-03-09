namespace HospitalMS.Web.Models;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;

    public string ErrorCode { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public string? Path { get; set; }

    // create error response
    public static ErrorResponse Create(string message, string errorCode, int statusCode, string? correlationId = null, Dictionary<string, string[]>? validationErrors = null, string? path = null)
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = errorCode,
            StatusCode = statusCode,
            CorrelationId = correlationId,
            ValidationErrors = validationErrors,
            Path = path,
            Timestamp = DateTime.UtcNow
        };
    }
}