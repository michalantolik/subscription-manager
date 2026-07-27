using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SubscriptionManager.Api.ExceptionHandling;

internal sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ArgumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request.",
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            },
            InvalidOperationException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "The operation cannot be completed.",
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            },
            _ => null
        };

        if (problemDetails is null)
        {
            return false;
        }

        httpContext.Response.StatusCode =
            problemDetails.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}
