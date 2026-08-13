namespace SubscriptionManager.Application.SavingsPlans;

/// <summary>
/// Indicates that the daily savings plan request limit has been reached.
/// </summary>
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
