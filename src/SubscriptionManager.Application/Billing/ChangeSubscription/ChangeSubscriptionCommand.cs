using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.ChangeSubscription;

/// <summary>
/// Request to change a billing subscription plan and billing interval.
/// </summary>
public sealed record ChangeSubscriptionCommand(
    SubscriptionPlan TargetPlan,
    BillingInterval TargetBillingInterval);
