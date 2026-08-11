using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing;

public sealed record PaymentSubscriptionState(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval,
    BillingSubscriptionStatus Status,
    string ProviderPriceId,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    bool CancelAtPeriodEnd);
