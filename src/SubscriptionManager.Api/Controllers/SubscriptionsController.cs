using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.CreateSubscription;
using SubscriptionManager.Application.Subscriptions.DeleteSubscription;
using SubscriptionManager.Application.Subscriptions.EndSubscription;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionById;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionCostSummary;
using SubscriptionManager.Application.Subscriptions.GetSubscriptions;
using SubscriptionManager.Application.Subscriptions.UpdateSubscription;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/subscriptions")]
public sealed class SubscriptionsController : ControllerBase
{
    private const string GetSubscriptionByIdRouteName =
        "GetSubscriptionById";

    private readonly CreateSubscriptionHandler _createSubscriptionHandler;
    private readonly GetSubscriptionsHandler _getSubscriptionsHandler;
    private readonly GetSubscriptionByIdHandler _getSubscriptionByIdHandler;
    private readonly GetSubscriptionCostSummaryHandler
        _getSubscriptionCostSummaryHandler;
    private readonly UpdateSubscriptionHandler _updateSubscriptionHandler;
    private readonly EndSubscriptionHandler _endSubscriptionHandler;
    private readonly DeleteSubscriptionHandler _deleteSubscriptionHandler;

    public SubscriptionsController(
        CreateSubscriptionHandler createSubscriptionHandler,
        GetSubscriptionsHandler getSubscriptionsHandler,
        GetSubscriptionByIdHandler getSubscriptionByIdHandler,
        GetSubscriptionCostSummaryHandler getSubscriptionCostSummaryHandler,
        UpdateSubscriptionHandler updateSubscriptionHandler,
        EndSubscriptionHandler endSubscriptionHandler,
        DeleteSubscriptionHandler deleteSubscriptionHandler)
    {
        _createSubscriptionHandler = createSubscriptionHandler;
        _getSubscriptionsHandler = getSubscriptionsHandler;
        _getSubscriptionByIdHandler = getSubscriptionByIdHandler;
        _getSubscriptionCostSummaryHandler =
            getSubscriptionCostSummaryHandler;
        _updateSubscriptionHandler = updateSubscriptionHandler;
        _endSubscriptionHandler = endSubscriptionHandler;
        _deleteSubscriptionHandler = deleteSubscriptionHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SubscriptionDto>>>
        GetAsync(
            CancellationToken cancellationToken)
    {
        var subscriptions =
            await _getSubscriptionsHandler.HandleAsync(
                cancellationToken);

        return Ok(subscriptions);
    }

    [HttpGet("cost-summary")]
    public async Task<ActionResult<SubscriptionCostSummaryDto>>
        GetCostSummaryAsync(
            CancellationToken cancellationToken)
    {
        var summary =
            await _getSubscriptionCostSummaryHandler.HandleAsync(
                cancellationToken);

        return Ok(summary);
    }

    [HttpGet("{id:guid}", Name = GetSubscriptionByIdRouteName)]
    public async Task<ActionResult<SubscriptionDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var subscription =
            await _getSubscriptionByIdHandler.HandleAsync(
                id,
                cancellationToken);

        if (subscription is null)
        {
            return SubscriptionNotFound(id);
        }

        return Ok(subscription);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAsync(
        CreateSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscriptionId =
                await _createSubscriptionHandler.HandleAsync(
                    command,
                    cancellationToken);

            return CreatedAtRoute(
                GetSubscriptionByIdRouteName,
                new { id = subscriptionId },
                subscriptionId);
        }
        catch (SubscriptionLimitReachedException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Subscription limit reached.",
                detail: exception.Message,
                instance: HttpContext.Request.Path,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "subscription_limit_reached",
                    ["limit"] = exception.Limit
                });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        UpdateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new UpdateSubscriptionCommand(
                id,
                request.Name,
                request.Amount,
                request.Currency,
                request.BillingPeriod,
                request.DigitalServiceId);

        var updated =
            await _updateSubscriptionHandler.HandleAsync(
                command,
                cancellationToken);

        if (!updated)
        {
            return SubscriptionNotFound(id);
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> EndAsync(
        Guid id,
        EndSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new EndSubscriptionCommand(
                id,
                request.EndDate);

        var ended =
            await _endSubscriptionHandler.HandleAsync(
                command,
                cancellationToken);

        if (!ended)
        {
            return SubscriptionNotFound(id);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command =
            new DeleteSubscriptionCommand(id);

        var deleted =
            await _deleteSubscriptionHandler.HandleAsync(
                command,
                cancellationToken);

        if (!deleted)
        {
            return SubscriptionNotFound(id);
        }

        return NoContent();
    }

    private ObjectResult SubscriptionNotFound(
        Guid id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Subscription not found.",
            detail:
                $"Subscription with id '{id}' was not found.",
            instance: HttpContext.Request.Path);
    }
}

public sealed record UpdateSubscriptionRequest(
    string Name,
    decimal Amount,
    Currency Currency,
    BillingPeriod BillingPeriod,
    Guid? DigitalServiceId = null);

public sealed record EndSubscriptionRequest(
    DateOnly EndDate);
