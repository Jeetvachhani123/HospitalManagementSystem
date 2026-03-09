using HospitalMS.BL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalMS.API.Filters;

public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            var response = ApiResponse<object>.ErrorResponse("Validation failed", errors);
            context.Result = new BadRequestObjectResult(response);
        }
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}