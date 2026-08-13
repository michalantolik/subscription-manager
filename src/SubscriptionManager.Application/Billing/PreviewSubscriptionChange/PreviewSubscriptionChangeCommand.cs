using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.PreviewSubscriptionChange;

/// <summary>
/// Request to preview a billing subscription change.
/// </summary>
public sealed record PreviewSubscriptionChangeCommand(
    SubscriptionPlan TargetPlan,
    BillingInterval TargetBillingInterval);
