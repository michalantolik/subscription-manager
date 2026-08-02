using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.ExchangeRates;

namespace SubscriptionManager.Api.ExceptionHandling;

internal sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ExchangeRatesUnavailableException => new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Exchange rates are unavailable.",
                Detail =
                    "Subscription costs could not be converted at this time.",
                Instance = httpContext.Request.Path
            },
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
            _ => CreateInternalServerError(httpContext)
        };

        if (exception is ExchangeRatesUnavailableException)
        {
            logger.LogWarning(
                exception,
                "Exchange rates were unavailable while processing {HttpMethod} {RequestPath}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else if (problemDetails.Status >=
                 StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "An unhandled exception occurred while processing {HttpMethod} {RequestPath}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode =
            problemDetails.Status!.Value;

        await problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });

        return true;
    }

    private static ProblemDetails CreateInternalServerError(
        HttpContext httpContext)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "The request could not be completed.",
            Instance = httpContext.Request.Path
        };
    }
}
