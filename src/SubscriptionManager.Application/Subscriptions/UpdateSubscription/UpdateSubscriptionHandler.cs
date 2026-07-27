using SubscriptionManager.Application.Common.Authentication;

namespace SubscriptionManager.Application.Subscriptions.UpdateSubscription;

public sealed class UpdateSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository,
        ICurrentUser currentUser)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> HandleAsync(
        UpdateSubscriptionCommand command,
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

        subscription.Update(
            command.Name,
            command.Amount,
            command.Currency,
            command.BillingPeriod);

        await _subscriptionRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
