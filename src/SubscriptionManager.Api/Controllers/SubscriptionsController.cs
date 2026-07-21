using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.CreateSubscription;
using SubscriptionManager.Application.Subscriptions.DeleteSubscription;
using SubscriptionManager.Application.Subscriptions.EndSubscription;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionById;
using SubscriptionManager.Application.Subscriptions.GetSubscriptions;
using SubscriptionManager.Application.Subscriptions.UpdateSubscription;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly CreateSubscriptionHandler _createSubscriptionHandler;
    private readonly GetSubscriptionsHandler _getSubscriptionsHandler;
    private readonly GetSubscriptionByIdHandler _getSubscriptionByIdHandler;
    private readonly UpdateSubscriptionHandler _updateSubscriptionHandler;
    private readonly EndSubscriptionHandler _endSubscriptionHandler;
    private readonly DeleteSubscriptionHandler _deleteSubscriptionHandler;

    public SubscriptionsController(
        CreateSubscriptionHandler createSubscriptionHandler,
        GetSubscriptionsHandler getSubscriptionsHandler,
        GetSubscriptionByIdHandler getSubscriptionByIdHandler,
        UpdateSubscriptionHandler updateSubscriptionHandler,
        EndSubscriptionHandler endSubscriptionHandler,
        DeleteSubscriptionHandler deleteSubscriptionHandler)
    {
        _createSubscriptionHandler = createSubscriptionHandler;
        _getSubscriptionsHandler = getSubscriptionsHandler;
        _getSubscriptionByIdHandler = getSubscriptionByIdHandler;
        _updateSubscriptionHandler = updateSubscriptionHandler;
        _endSubscriptionHandler = endSubscriptionHandler;
        _deleteSubscriptionHandler = deleteSubscriptionHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SubscriptionDto>>> GetAsync(
        CancellationToken cancellationToken)
    {
        var subscriptions = await _getSubscriptionsHandler.HandleAsync(
            cancellationToken);

        return Ok(subscriptions);
    }

    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
    public async Task<ActionResult<SubscriptionDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var subscription = await _getSubscriptionByIdHandler.HandleAsync(
            id,
            cancellationToken);

        if (subscription is null)
        {
            return NotFound();
        }

        return Ok(subscription);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAsync(
        CreateSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var subscriptionId = await _createSubscriptionHandler.HandleAsync(
            command,
            cancellationToken);

        return CreatedAtRoute(
            nameof(GetByIdAsync),
            new { id = subscriptionId },
            subscriptionId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        UpdateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSubscriptionCommand(
            id,
            request.Name,
            request.Amount,
            request.Currency,
            request.BillingPeriod);

        var updated = await _updateSubscriptionHandler.HandleAsync(
            command,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> EndAsync(
        Guid id,
        EndSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EndSubscriptionCommand(
            id,
            request.EndDate);

        var ended = await _endSubscriptionHandler.HandleAsync(
            command,
            cancellationToken);

        if (!ended)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSubscriptionCommand(id);

        var deleted = await _deleteSubscriptionHandler.HandleAsync(
            command,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

public sealed record UpdateSubscriptionRequest(
    string Name,
    decimal Amount,
    string Currency,
    BillingPeriod BillingPeriod);

public sealed record EndSubscriptionRequest(
    DateOnly EndDate);
