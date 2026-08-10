using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.GetBillingOverview;

public sealed record BillingOverviewDto(
    SubscriptionPlan Plan,
    BillingInterval? BillingInterval,
    BillingSubscriptionStatus? Status,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    bool CancelAtPeriodEnd);
