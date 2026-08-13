using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions.CreateSubscription;

/// <summary>
/// Request to create a subscription.
/// </summary>
public sealed record CreateSubscriptionCommand(
    string Name,
    decimal Amount,
    Currency Currency,
    BillingPeriod BillingPeriod,
    DateOnly StartDate,
    Guid? DigitalServiceId = null);
