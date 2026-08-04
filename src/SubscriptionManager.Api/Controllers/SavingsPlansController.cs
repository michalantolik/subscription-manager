using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/savings-plans")]
public sealed class SavingsPlansController
    : ControllerBase
{
    private readonly CreateSavingsPlanHandler
        _createSavingsPlanHandler;

    public SavingsPlansController(
        CreateSavingsPlanHandler createSavingsPlanHandler)
    {
        _createSavingsPlanHandler =
            createSavingsPlanHandler;
    }

    [HttpPost]
    public async Task<ActionResult<SavingsPlanDto>> CreateAsync(
        CreateSavingsPlanCommand command,
        CancellationToken cancellationToken)
    {
        var savingsPlan =
            await _createSavingsPlanHandler.HandleAsync(
                command,
                cancellationToken);

        return Ok(savingsPlan);
    }
}
