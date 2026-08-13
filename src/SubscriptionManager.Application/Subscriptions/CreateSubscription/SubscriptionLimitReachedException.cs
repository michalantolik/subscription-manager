namespace SubscriptionManager.Application.Subscriptions.CreateSubscription;

/// <summary>
/// Indicates that the active subscription limit has been reached.
/// </summary>
public sealed class SubscriptionLimitReachedException
    : Exception
{
    public SubscriptionLimitReachedException(
        int limit)
        : base(
            $"The active subscription limit of {limit} has been reached.")
    {
        Limit = limit;
    }

    public int Limit { get; }
}
