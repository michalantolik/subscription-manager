using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.PreviewSubscriptionChange;

/// <summary>
/// Billing subscription change preview data returned by the preview use case.
/// </summary>
public sealed record SubscriptionChangePreviewDto(
    SubscriptionPlan CurrentPlan,
    BillingInterval CurrentBillingInterval,
    SubscriptionPlan TargetPlan,
    BillingInterval TargetBillingInterval,
    BillingSubscriptionChangeTiming Timing,
    decimal AmountDueNow,
    string Currency,
    DateTimeOffset EffectiveAt);
