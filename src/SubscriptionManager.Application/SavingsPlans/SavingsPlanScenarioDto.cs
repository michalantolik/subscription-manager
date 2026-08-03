namespace SubscriptionManager.Application.SavingsPlans;

public sealed record SavingsPlanScenarioDto(
    IReadOnlyCollection<SavingsPlanSubscriptionDto> Subscriptions,
    decimal ProjectedMonthlyCost,
    decimal MonthlySavings,
    decimal YearlySavings,
    bool TargetReached,
    string Explanation);
