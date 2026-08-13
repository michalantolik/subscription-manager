namespace SubscriptionManager.Web.Features.Billing;

/// <summary>
/// Billing plan price returned by the Billing API.
/// </summary>
public sealed record PaymentPlanPriceResponse(
    BillingPlan Plan,
    BillingInterval BillingInterval,
    decimal Amount,
    string Currency);

/// <summary>
/// Billing overview returned by the Billing API.
/// </summary>
public sealed record BillingOverviewResponse(
    BillingPlan Plan,
    BillingInterval? BillingInterval,
    BillingSubscriptionStatus? Status,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    bool CancelAtPeriodEnd);

/// <summary>
/// Subscription change preview returned by the Billing API.
/// </summary>
public sealed record SubscriptionChangePreviewResponse(
    BillingPlan CurrentPlan,
    BillingInterval CurrentBillingInterval,
    BillingPlan TargetPlan,
    BillingInterval TargetBillingInterval,
    BillingSubscriptionChangeTiming Timing,
    decimal AmountDueNow,
    string Currency,
    DateTimeOffset EffectiveAt);

/// <summary>
/// Checkout session data returned by the Billing API.
/// </summary>
public sealed record CreateCheckoutSessionResponse(
    string CheckoutUrl);
