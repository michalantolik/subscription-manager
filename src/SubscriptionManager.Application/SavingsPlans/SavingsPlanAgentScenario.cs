namespace SubscriptionManager.Application.SavingsPlans;

public sealed record SavingsPlanAgentScenario(
    IReadOnlyCollection<Guid> SubscriptionIds,
    string Explanation);
