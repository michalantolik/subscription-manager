using System.Text.Json.Serialization;

namespace SubscriptionManager.Web.Features.Billing;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingPlan
{
    Free = 1,
    Plus = 2,
    Premium = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingInterval
{
    Monthly = 1,
    Yearly = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingSubscriptionStatus
{
    Incomplete = 1,
    Active = 2,
    PastDue = 3,
    Canceled = 4,
    Unpaid = 5,
    Paused = 6,
    IncompleteExpired = 7
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingSubscriptionChangeTiming
{
    Immediate = 1,
    NextBillingPeriod = 2
}

public sealed record PaymentPlanPriceResponse(
    BillingPlan Plan,
    BillingInterval BillingInterval,
    decimal Amount,
    string Currency);

public sealed record BillingOverviewResponse(
    BillingPlan Plan,
    BillingInterval? BillingInterval,
    BillingSubscriptionStatus? Status,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    bool CancelAtPeriodEnd);

public sealed record SubscriptionChangePreviewResponse(
    BillingPlan CurrentPlan,
    BillingInterval CurrentBillingInterval,
    BillingPlan TargetPlan,
    BillingInterval TargetBillingInterval,
    BillingSubscriptionChangeTiming Timing,
    decimal AmountDueNow,
    string Currency,
    DateTimeOffset EffectiveAt);

public sealed record CreateCheckoutSessionResponse(
    string CheckoutUrl);
