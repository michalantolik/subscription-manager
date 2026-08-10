using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.CreateCheckoutSession;

public sealed record CreateCheckoutSessionCommand(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval,
    string SuccessUrl,
    string CancelUrl);
