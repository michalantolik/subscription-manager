namespace SubscriptionManager.Infrastructure.SavingsPlans.OpenAi;

/// <summary>
/// Represents the result of a savings plan simulation.
/// </summary>
internal sealed record SavingsPlanSimulationResult(
    bool IsValid,
    string? Error,
    IReadOnlyCollection<Guid> SubscriptionIds,
    decimal ProjectedMonthlyCost,
    decimal MonthlySavings,
    bool TargetReached);
