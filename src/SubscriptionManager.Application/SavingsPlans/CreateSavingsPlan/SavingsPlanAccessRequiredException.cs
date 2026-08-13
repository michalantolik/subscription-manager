namespace SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;

/// <summary>
/// Indicates that savings plan access is not available for the current subscription plan.
/// </summary>
public sealed class SavingsPlanAccessRequiredException
    : Exception
{
    public SavingsPlanAccessRequiredException()
        : base(
            "The current subscription plan does not include savings plan generation.")
    {
    }
}
