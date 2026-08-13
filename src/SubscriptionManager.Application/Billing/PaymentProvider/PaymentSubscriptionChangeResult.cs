namespace SubscriptionManager.Application.Billing.PaymentProvider;

/// <summary>
/// Billing subscription change result provided by the payment provider.
/// </summary>
public sealed record PaymentSubscriptionChangeResult(
    PaymentSubscriptionState? UpdatedSubscription);
