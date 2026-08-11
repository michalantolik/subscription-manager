namespace SubscriptionManager.Application.Billing;

public sealed record PaymentSubscriptionChangePreview(
    decimal AmountDueNow,
    string Currency,
    DateTimeOffset EffectiveAt);
