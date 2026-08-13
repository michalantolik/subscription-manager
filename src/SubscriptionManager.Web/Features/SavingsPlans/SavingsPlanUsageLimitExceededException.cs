namespace SubscriptionManager.Web.Features.SavingsPlans;

/// <summary>
/// Represents an attempt to exceed the daily savings plan usage limit.
/// </summary>
public sealed class SavingsPlanUsageLimitExceededException
    : Exception
{
    public SavingsPlanUsageLimitExceededException(
        string? message,
        int dailyLimit)
        : base(
            string.IsNullOrWhiteSpace(message)
                ? "The daily savings plan limit has been reached."
                : message)
    {
        DailyLimit = dailyLimit;
    }

    public int DailyLimit { get; }
}
