using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Billing.ProcessWebhook;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Application.SavingsPlans;

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
            InvalidPaymentWebhookException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid payment webhook.",
                Detail =
                    "The payment webhook could not be verified or processed.",
                Instance = httpContext.Request.Path
            },

            ExchangeRatesUnavailableException => new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Exchange rates are unavailable.",
                Detail =
                    "Subscription costs could not be converted at this time.",
                Instance = httpContext.Request.Path
            },

            SavingsPlanUsageLimitExceededException limitException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Daily savings plan limit reached.",
                    Detail = limitException.Message,
                    Instance = httpContext.Request.Path,
                    Extensions =
                    {
                        ["dailyLimit"] =
                            limitException.DailyLimit
                    }
                },

            SavingsPlanUnavailableException => new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Savings plan is unavailable.",
                Detail =
                    "The savings plan could not be generated at this time. Please try again later.",
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

            _ => CreateInternalServerError(
                httpContext)
        };

        if (exception is ExchangeRatesUnavailableException)
        {
            logger.LogWarning(
                exception,
                "Exchange rates were unavailable while processing {HttpMethod} {RequestPath}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else if (exception is SavingsPlanUnavailableException)
        {
            logger.LogWarning(
                exception,
                "The savings plan provider was unavailable while processing {HttpMethod} {RequestPath}.",
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
            Status =
                StatusCodes.Status500InternalServerError,
            Title =
                "An unexpected error occurred.",
            Detail =
                "The request could not be completed.",
            Instance =
                httpContext.Request.Path
        };
    }
}
