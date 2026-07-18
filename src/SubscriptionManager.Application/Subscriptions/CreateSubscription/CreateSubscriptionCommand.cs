using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    string Name,
    decimal Amount,
    string Currency,
    BillingPeriod BillingPeriod,
    DateOnly StartDate);
