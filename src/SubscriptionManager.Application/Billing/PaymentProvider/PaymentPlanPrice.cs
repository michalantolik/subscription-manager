using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.PaymentProvider;

/// <summary>
/// Payment plan price provided by the payment provider.
/// </summary>
public sealed record PaymentPlanPrice(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval,
    decimal Amount,
    string Currency);
