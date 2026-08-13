using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.CreateCheckoutSession;

/// <summary>
/// Request to create a checkout session for a billing subscription.
/// </summary>
public sealed record CreateCheckoutSessionCommand(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval,
    string SuccessUrl,
    string CancelUrl);
