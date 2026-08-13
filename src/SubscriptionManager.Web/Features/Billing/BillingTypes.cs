using System.Text.Json.Serialization;

namespace SubscriptionManager.Web.Features.Billing;

/// <summary>
/// Represents a subscription plan available in the web application.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingPlan
{
    Free = 1,
    Plus = 2,
    Premium = 3
}

/// <summary>
/// Represents a billing interval supported by the web application.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingInterval
{
    Monthly = 1,
    Yearly = 2
}

/// <summary>
/// Represents the status of a billing subscription.
/// </summary>
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

/// <summary>
/// Represents when a billing subscription change takes effect.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingSubscriptionChangeTiming
{
    Immediate = 1,
    NextBillingPeriod = 2
}
