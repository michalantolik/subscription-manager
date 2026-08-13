namespace SubscriptionManager.Application.SavingsPlans.Ai;

/// <summary>
/// Savings plan data returned by the AI service.
/// </summary>
public sealed record SavingsPlanAgentResult(
    SavingsPlanAgentScenario? Recommended,
    SavingsPlanAgentScenario? Alternative);
