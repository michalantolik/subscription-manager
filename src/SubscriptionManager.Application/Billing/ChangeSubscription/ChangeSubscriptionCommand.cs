using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.ChangeSubscription;

public sealed record ChangeSubscriptionCommand(
    SubscriptionPlan TargetPlan,
    BillingInterval TargetBillingInterval);
