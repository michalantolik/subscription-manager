namespace SubscriptionManager.Application.Subscriptions.EndSubscription;

public sealed class EndSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public EndSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<bool> HandleAsync(
        EndSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(
            command.SubscriptionId,
            cancellationToken);

        if (subscription is null)
        {
            return false;
        }

        subscription.End(command.EndDate);

        await _subscriptionRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
