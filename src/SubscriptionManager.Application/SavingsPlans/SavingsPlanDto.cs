using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.SavingsPlans;

/// <summary>
/// Savings plan data returned by savings plan use cases.
/// </summary>
public sealed record SavingsPlanDto(
    Currency BaseCurrency,
    decimal CurrentMonthlyCost,
    SavingsPlanScenarioDto? Recommended,
    SavingsPlanScenarioDto? Alternative,
    SubscriptionPlan SubscriptionPlan,
    int DailyRequestLimit,
    int RemainingRequestCount);
