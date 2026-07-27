using SubscriptionManager.Application.Common.Authentication;

namespace SubscriptionManager.Application.Subscriptions.DeleteSubscription;

public sealed class DeleteSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository,
        ICurrentUser currentUser)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> HandleAsync(
        DeleteSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var subscription = await _subscriptionRepository.GetByIdAsync(
            command.SubscriptionId,
            _currentUser.UserId,
            cancellationToken);

        if (subscription is null)
        {
            return false;
        }

        _subscriptionRepository.Remove(subscription);

        await _subscriptionRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
