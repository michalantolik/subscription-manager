namespace SubscriptionManager.Application.Subscriptions.EndSubscription;

public sealed record EndSubscriptionCommand(
    Guid SubscriptionId,
    DateOnly EndDate);
