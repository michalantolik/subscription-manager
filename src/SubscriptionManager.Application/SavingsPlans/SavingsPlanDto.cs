using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.SavingsPlans;

public sealed record SavingsPlanDto(
    Currency BaseCurrency,
    decimal CurrentMonthlyCost,
    SavingsPlanScenarioDto? Recommended,
    SavingsPlanScenarioDto? Alternative);
