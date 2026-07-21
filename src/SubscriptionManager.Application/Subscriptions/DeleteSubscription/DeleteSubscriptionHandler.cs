namespace SubscriptionManager.Application.Subscriptions.DeleteSubscription;

public sealed class DeleteSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public DeleteSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<bool> HandleAsync(
        DeleteSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(
            command.SubscriptionId,
            cancellationToken);

        if (subscription is null)
        {
            return false;
        }

        _subscriptionRepository.Remove(subscription);

        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
