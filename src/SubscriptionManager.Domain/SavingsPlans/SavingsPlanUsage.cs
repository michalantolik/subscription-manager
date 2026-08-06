namespace SubscriptionManager.Domain.SavingsPlans;

public sealed class SavingsPlanUsage
{
    private SavingsPlanUsage()
    {
    }

    public SavingsPlanUsage(
        Guid userId,
        DateOnly usageDateUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier is required.",
                nameof(userId));
        }

        if (usageDateUtc == default)
        {
            throw new ArgumentException(
                "Usage date is required.",
                nameof(usageDateUtc));
        }

        UserId = userId;
        UsageDateUtc = usageDateUtc;
    }

    public Guid UserId { get; private set; }

    public DateOnly UsageDateUtc { get; private set; }

    public int RequestCount { get; private set; }

    public bool HasReachedLimit(
        int dailyLimit)
    {
        if (dailyLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyLimit));
        }

        return RequestCount >= dailyLimit;
    }

    public int GetRemainingRequestCount(
        int dailyLimit)
    {
        if (dailyLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyLimit));
        }

        return Math.Max(
            0,
            dailyLimit - RequestCount);
    }

    public void RegisterRequest(
        int dailyLimit)
    {
        if (dailyLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyLimit));
        }

        if (HasReachedLimit(dailyLimit))
        {
            throw new InvalidOperationException(
                "The daily savings plan request limit has been reached.");
        }

        RequestCount++;
    }
}
