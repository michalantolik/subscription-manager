using SubscriptionManager.Web.Common.Currencies;
using System.Text.Json.Serialization;

namespace SubscriptionManager.Web.Features.SavingsPlans;

/// <summary>
/// Represents the goal of a savings plan.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SavingsPlanGoalType
{
    MonthlyBudget = 1,
    MonthlySavings = 2
}

/// <summary>
/// Represents a strategy for generating a savings plan.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SavingsPlanStrategy
{
    FewerChanges = 1,
    Balanced = 2,
    MaximumSavings = 3
}

/// <summary>
/// Represents a subscription plan available in the web application.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionPlan
{
    Free = 1,
    Plus = 2,
    Premium = 3
}

/// <summary>
/// Savings plan data sent to the Savings Plans API.
/// </summary>
public sealed record CreateSavingsPlanRequest(
    SavingsPlanGoalType GoalType,
    decimal TargetAmount,
    IReadOnlyCollection<Guid> ProtectedSubscriptionIds,
    SavingsPlanStrategy Strategy,
    string? AdditionalPreference,
    string LanguageCode);

/// <summary>
/// Savings plan usage data returned by the Savings Plans API.
/// </summary>
public sealed record SavingsPlanUsageResponse(
    SubscriptionPlan SubscriptionPlan,
    int DailyRequestLimit,
    int RemainingRequestCount);

/// <summary>
/// Savings plan data returned by the Savings Plans API.
/// </summary>
public sealed record SavingsPlanResponse(
    Currency BaseCurrency,
    decimal CurrentMonthlyCost,
    SavingsPlanScenarioResponse? Recommended,
    SavingsPlanScenarioResponse? Alternative,
    SubscriptionPlan SubscriptionPlan,
    int DailyRequestLimit,
    int RemainingRequestCount);

/// <summary>
/// Savings plan scenario returned by the Savings Plans API.
/// </summary>
public sealed record SavingsPlanScenarioResponse(
    IReadOnlyList<SavingsPlanSubscriptionResponse> Subscriptions,
    decimal ProjectedMonthlyCost,
    decimal MonthlySavings,
    decimal YearlySavings,
    bool TargetReached,
    string Explanation);

/// <summary>
/// Subscription data returned as part of a savings plan scenario.
/// </summary>
public sealed record SavingsPlanSubscriptionResponse(
    Guid Id,
    string Name,
    string Category,
    decimal MonthlyCost);
