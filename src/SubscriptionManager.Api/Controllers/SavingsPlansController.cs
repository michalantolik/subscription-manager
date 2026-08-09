using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;
using SubscriptionManager.Application.SavingsPlans.GetSavingsPlanUsage;

namespace SubscriptionManager.Api.Controllers;

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
        var savingsPlan =
            await createSavingsPlanHandler.HandleAsync(
                command,
                cancellationToken);

        return Ok(savingsPlan);
    }
}
