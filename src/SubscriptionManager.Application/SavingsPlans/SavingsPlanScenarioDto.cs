namespace SubscriptionManager.Application.SavingsPlans;

/// <summary>
/// Savings plan scenario data returned by savings plan use cases.
/// </summary>
public sealed record SavingsPlanScenarioDto(
    IReadOnlyCollection<SavingsPlanSubscriptionDto> Subscriptions,
    decimal ProjectedMonthlyCost,
    decimal MonthlySavings,
    decimal YearlySavings,
    bool TargetReached,
    string Explanation);
