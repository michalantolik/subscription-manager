namespace SubscriptionManager.Application.Subscriptions.EndSubscription;

/// <summary>
/// Request to end a subscription.
/// </summary>
public sealed record EndSubscriptionCommand(
    Guid SubscriptionId,
    DateOnly EndDate);
