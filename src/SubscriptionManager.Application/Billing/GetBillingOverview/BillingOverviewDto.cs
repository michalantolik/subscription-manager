using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.GetBillingOverview;

/// <summary>
/// Billing subscription overview data returned by the billing overview use case.
/// </summary>
public sealed record BillingOverviewDto(
    SubscriptionPlan Plan,
    BillingInterval? BillingInterval,
    BillingSubscriptionStatus? Status,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    bool CancelAtPeriodEnd);
