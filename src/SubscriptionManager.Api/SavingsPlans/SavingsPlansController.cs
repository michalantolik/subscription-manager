using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;
using SubscriptionManager.Application.SavingsPlans.GetSavingsPlanUsage;

namespace SubscriptionManager.Api.SavingsPlans;

/// <summary>
/// Exposes savings plan use cases through HTTP endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/savings-plans")]
public sealed class SavingsPlansController
    : ControllerBase
{
    private readonly GetSavingsPlanUsageHandler
        _getSavingsPlanUsageHandler;

    public SavingsPlansController(
        GetSavingsPlanUsageHandler getSavingsPlanUsageHandler)
    {
        _getSavingsPlanUsageHandler =
            getSavingsPlanUsageHandler;
    }

    [HttpGet("usage")]
    public async Task<ActionResult<SavingsPlanUsageDto>>
        GetUsageAsync(
            CancellationToken cancellationToken)
    {
        var usage =
            await _getSavingsPlanUsageHandler.HandleAsync(
                cancellationToken);

        return Ok(usage);
    }

    [HttpPost]
    public async Task<ActionResult<SavingsPlanDto>> CreateAsync(
        CreateSavingsPlanCommand command,
        [FromServices]
        CreateSavingsPlanHandler createSavingsPlanHandler,
        CancellationToken cancellationToken)
    {
        try
        {
            var savingsPlan =
                await createSavingsPlanHandler.HandleAsync(
                    command,
                    cancellationToken);

            return Ok(savingsPlan);
        }
        catch (SavingsPlanAccessRequiredException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Savings plan access required.",
                detail: exception.Message,
                instance: HttpContext.Request.Path,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "savings_plan_access_required"
                });
        }
        catch (SavingsPlanUsageLimitExceededException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status429TooManyRequests,
                title: "Savings plan usage limit exceeded.",
                detail: exception.Message,
                instance: HttpContext.Request.Path,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "savings_plan_usage_limit_exceeded",
                    ["limit"] = exception.DailyLimit
                });
        }
    }
}
