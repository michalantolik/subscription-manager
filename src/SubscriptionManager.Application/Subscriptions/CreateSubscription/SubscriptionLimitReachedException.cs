namespace SubscriptionManager.Application.Subscriptions.CreateSubscription;

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
