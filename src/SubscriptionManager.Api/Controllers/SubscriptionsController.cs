using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Subscriptions.CreateSubscription;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly CreateSubscriptionHandler _createSubscriptionHandler;

    public SubscriptionsController(
        CreateSubscriptionHandler createSubscriptionHandler)
    {
        _createSubscriptionHandler = createSubscriptionHandler;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAsync(
        CreateSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var subscriptionId = await _createSubscriptionHandler.HandleAsync(
            command,
            cancellationToken);

        return Created(
            $"/api/subscriptions/{subscriptionId}",
            subscriptionId);
    }
}
