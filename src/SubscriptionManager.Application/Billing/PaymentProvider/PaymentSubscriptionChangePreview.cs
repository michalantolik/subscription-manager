namespace SubscriptionManager.Application.Billing.PaymentProvider;

/// <summary>
/// Billing subscription change preview provided by the payment provider.
/// </summary>
public sealed record PaymentSubscriptionChangePreview(
    decimal AmountDueNow,
    string Currency,
    DateTimeOffset EffectiveAt);
