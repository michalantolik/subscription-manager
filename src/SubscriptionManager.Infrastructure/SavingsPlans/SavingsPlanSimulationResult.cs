namespace SubscriptionManager.Infrastructure.SavingsPlans;

internal sealed record SavingsPlanSimulationResult(
    bool IsValid,
    string? Error,
    IReadOnlyCollection<Guid> SubscriptionIds,
    decimal ProjectedMonthlyCost,
    decimal MonthlySavings,
    bool TargetReached);
