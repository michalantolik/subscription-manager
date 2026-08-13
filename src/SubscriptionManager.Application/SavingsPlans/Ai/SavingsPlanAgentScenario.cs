namespace SubscriptionManager.Application.SavingsPlans.Ai;

/// <summary>
/// Savings plan scenario returned by the AI service.
/// </summary>
public sealed record SavingsPlanAgentScenario(
    IReadOnlyCollection<Guid> SubscriptionIds,
    string Explanation);
