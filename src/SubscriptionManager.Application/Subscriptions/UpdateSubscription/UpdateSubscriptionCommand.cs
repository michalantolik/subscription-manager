using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions.UpdateSubscription;

/// <summary>
/// Request to update a subscription.
/// </summary>
public sealed record UpdateSubscriptionCommand(
    Guid SubscriptionId,
    string Name,
    decimal Amount,
    Currency Currency,
    BillingPeriod BillingPeriod,
    Guid? DigitalServiceId = null);
