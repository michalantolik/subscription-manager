using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.PaymentProvider;

/// <summary>
/// Billing subscription state provided by the payment provider.
/// </summary>
public sealed record PaymentSubscriptionState(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval,
    BillingSubscriptionStatus Status,
    string ProviderPriceId,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    bool CancelAtPeriodEnd);
