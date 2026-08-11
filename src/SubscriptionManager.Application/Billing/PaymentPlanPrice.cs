using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing;

public sealed record PaymentPlanPrice(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval,
    decimal Amount,
    string Currency);
