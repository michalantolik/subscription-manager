using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Subscriptions.EndSubscription;

/// <summary>
/// Handles subscription ending.
/// </summary>
public sealed class EndSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICurrentUser _currentUser;

    public EndSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository,
        ICurrentUser currentUser)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> HandleAsync(
        EndSubscriptionCommand command,
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

        subscription.End(command.EndDate);

        await _subscriptionRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
