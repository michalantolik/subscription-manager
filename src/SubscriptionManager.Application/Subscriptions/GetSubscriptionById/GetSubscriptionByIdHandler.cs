using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Subscriptions.GetSubscriptionById;

/// <summary>
/// Handles subscription retrieval by identifier.
/// </summary>
public sealed class GetSubscriptionByIdHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICurrentUser _currentUser;

    public GetSubscriptionByIdHandler(
        ISubscriptionRepository subscriptionRepository,
        ICurrentUser currentUser)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentUser = currentUser;
    }

    public async Task<SubscriptionDto?> HandleAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(
            subscriptionId,
            _currentUser.UserId,
            cancellationToken);

        return subscription?.ToDto();
    }
}
