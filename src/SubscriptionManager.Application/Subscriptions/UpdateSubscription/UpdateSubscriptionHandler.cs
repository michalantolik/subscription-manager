namespace SubscriptionManager.Application.Subscriptions.UpdateSubscription;

public sealed class UpdateSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public UpdateSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<bool> HandleAsync(
        UpdateSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(
            command.SubscriptionId,
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
