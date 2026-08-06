namespace SubscriptionManager.Application.SavingsPlans;

public sealed class SavingsPlanUsageLimitExceededException
    : Exception
{
    public SavingsPlanUsageLimitExceededException(
        int dailyLimit)
        : base(
            $"The daily savings plan limit of {dailyLimit} requests has been reached.")
    {
        DailyLimit = dailyLimit;
    }

    public int DailyLimit { get; }
}
