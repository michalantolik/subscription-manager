using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions.UpdateSubscription;

public sealed record UpdateSubscriptionCommand(
    Guid SubscriptionId,
    string Name,
    decimal Amount,
    string Currency,
    BillingPeriod BillingPeriod);
