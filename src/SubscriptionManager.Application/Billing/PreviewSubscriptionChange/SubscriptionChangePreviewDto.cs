using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.PreviewSubscriptionChange;

public sealed record SubscriptionChangePreviewDto(
    SubscriptionPlan CurrentPlan,
    BillingInterval CurrentBillingInterval,
    SubscriptionPlan TargetPlan,
    BillingInterval TargetBillingInterval,
    BillingSubscriptionChangeTiming Timing,
    decimal AmountDueNow,
    string Currency,
    DateTimeOffset EffectiveAt);
