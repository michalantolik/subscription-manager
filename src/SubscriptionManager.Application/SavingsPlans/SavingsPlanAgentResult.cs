namespace SubscriptionManager.Application.SavingsPlans;

public sealed record SavingsPlanAgentResult(
    SavingsPlanAgentScenario? Recommended,
    SavingsPlanAgentScenario? Alternative);
