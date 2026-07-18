using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions.CreateSubscription;

public sealed class CreateSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public CreateSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Guid> HandleAsync(
        CreateSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var subscription = new Subscription(
            Guid.NewGuid(),
            command.Name,
            command.Amount,
            command.Currency,
            command.BillingPeriod,
            command.StartDate);

        await _subscriptionRepository.AddAsync(
            subscription,
            cancellationToken);

        await _subscriptionRepository.SaveChangesAsync(
            cancellationToken);

        return subscription.Id;
    }
}
