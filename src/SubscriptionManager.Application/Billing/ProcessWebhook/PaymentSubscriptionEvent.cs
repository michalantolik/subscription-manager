using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.ProcessWebhook;

/// <summary>
/// Billing subscription event received from the payment provider.
/// </summary>
public sealed record PaymentSubscriptionEvent(
    string ProviderEventId,
    DateTimeOffset ProviderEventCreatedAt,
    Guid? UserId,
    string ProviderCustomerId,
    string ProviderSubscriptionId,
    string ProviderPriceId,
    SubscriptionPlan Plan,
    BillingInterval BillingInterval,
    BillingSubscriptionStatus Status,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    bool CancelAtPeriodEnd);
