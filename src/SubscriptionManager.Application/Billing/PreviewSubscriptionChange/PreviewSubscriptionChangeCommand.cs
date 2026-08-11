using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.PreviewSubscriptionChange;

public sealed record PreviewSubscriptionChangeCommand(
    SubscriptionPlan TargetPlan,
    BillingInterval TargetBillingInterval);
