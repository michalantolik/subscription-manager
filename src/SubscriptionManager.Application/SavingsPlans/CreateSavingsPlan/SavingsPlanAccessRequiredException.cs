namespace SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;

public sealed class SavingsPlanAccessRequiredException
    : Exception
{
    public SavingsPlanAccessRequiredException()
        : base(
            "The current subscription plan does not include savings plan generation.")
    {
    }
}
