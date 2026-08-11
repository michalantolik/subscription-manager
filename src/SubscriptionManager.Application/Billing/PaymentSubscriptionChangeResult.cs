namespace SubscriptionManager.Application.Billing;

public sealed record PaymentSubscriptionChangeResult(
    PaymentSubscriptionState? UpdatedSubscription);
