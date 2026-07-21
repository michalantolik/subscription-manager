namespace SubscriptionManager.Application.Subscriptions.GetSubscriptionById;

public sealed class GetSubscriptionByIdHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionByIdHandler(
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<SubscriptionDto?> HandleAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(
            subscriptionId,
            cancellationToken);

        return subscription?.ToDto();
    }
}
