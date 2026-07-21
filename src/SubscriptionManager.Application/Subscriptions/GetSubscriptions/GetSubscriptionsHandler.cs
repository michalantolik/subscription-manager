namespace SubscriptionManager.Application.Subscriptions.GetSubscriptions;

public sealed class GetSubscriptionsHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionsHandler(
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<IReadOnlyCollection<SubscriptionDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync(
            cancellationToken);

        return subscriptions
            .Select(subscription => subscription.ToDto())
            .ToArray();
    }
}
