namespace SubscriptionManager.Application.SavingsPlans.Ai;

/// <summary>
/// Generates savings plans using an AI service.
/// </summary>
public interface ISavingsPlanAgent
{
    Task<SavingsPlanAgentResult> CreatePlanAsync(
        SavingsPlanAgentRequest request,
        CancellationToken cancellationToken = default);
}
