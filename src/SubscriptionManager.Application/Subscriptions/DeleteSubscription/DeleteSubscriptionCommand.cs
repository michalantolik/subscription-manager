namespace SubscriptionManager.Application.Subscriptions.DeleteSubscription;

/// <summary>
/// Request to delete a subscription.
/// </summary>
public sealed record DeleteSubscriptionCommand(
    Guid SubscriptionId);
