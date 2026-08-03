namespace SubscriptionManager.Application.SavingsPlans;

public interface ISavingsPlanAgent
{
    Task<SavingsPlanAgentResult> CreatePlanAsync(
        SavingsPlanAgentRequest request,
        CancellationToken cancellationToken = default);
}
