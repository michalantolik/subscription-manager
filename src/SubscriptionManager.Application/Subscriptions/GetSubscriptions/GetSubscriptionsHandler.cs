using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Subscriptions.GetSubscriptions;

public sealed class GetSubscriptionsHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICurrentUser _currentUser;

    public GetSubscriptionsHandler(
        ISubscriptionRepository subscriptionRepository,
        ICurrentUser currentUser)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<SubscriptionDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync(
            _currentUser.UserId,
            cancellationToken);

        return subscriptions
            .Select(subscription => subscription.ToDto())
            .ToArray();
    }
}
