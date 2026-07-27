using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions.CreateSubscription;

public sealed class CreateSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IDigitalServiceRepository _digitalServiceRepository;
    private readonly ICurrentUser _currentUser;

    public CreateSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository,
        IDigitalServiceRepository digitalServiceRepository,
        ICurrentUser currentUser)
    {
        _subscriptionRepository = subscriptionRepository;
        _digitalServiceRepository = digitalServiceRepository;
        _currentUser = currentUser;
    }

    public async Task<Guid> HandleAsync(
        CreateSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ownerId = _currentUser.UserId;

        DigitalService? digitalService = null;

        if (command.DigitalServiceId is not null)
        {
            if (command.DigitalServiceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Digital service identifier cannot be empty.",
                    nameof(command.DigitalServiceId));
            }

            digitalService =
                await _digitalServiceRepository.GetAvailableByIdAsync(
                    command.DigitalServiceId.Value,
                    ownerId,
                    cancellationToken);

            if (digitalService is null)
            {
                throw new ArgumentException(
                    "The selected digital service is not available.",
                    nameof(command.DigitalServiceId));
            }
        }

        var subscription = new Subscription(
            Guid.NewGuid(),
            ownerId,
            command.Name,
            command.Amount,
            command.Currency,
            command.BillingPeriod,
            command.StartDate);

        if (digitalService is not null)
        {
            subscription.AssignDigitalService(
                digitalService.Id,
                digitalService.Category,
                digitalService.CustomCategoryName,
                digitalService.IconKey,
                digitalService.ManagementUrl);
        }

        await _subscriptionRepository.AddAsync(
            subscription,
            cancellationToken);

        await _subscriptionRepository.SaveChangesAsync(
            cancellationToken);

        return subscription.Id;
    }
}
