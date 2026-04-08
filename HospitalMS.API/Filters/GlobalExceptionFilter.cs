using HospitalMS.BL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalMS.API.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    // handle unhandled exceptions
    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "An unhandled exception occurred");
        var response = ApiResponse<object>.ErrorResponse("An error occurred while processing your request", context.Exception.Message);
        
        context.Result = new ObjectResult(response)
        {
            StatusCode = 500
        };

        context.ExceptionHandled = true;
    }
}