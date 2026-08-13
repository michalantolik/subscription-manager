using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.DigitalServices;

namespace SubscriptionManager.Application.Subscriptions.UpdateSubscription;

/// <summary>
/// Handles subscription update.
/// </summary>
public sealed class UpdateSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IDigitalServiceRepository _digitalServiceRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateSubscriptionHandler(
        ISubscriptionRepository subscriptionRepository,
        IDigitalServiceRepository digitalServiceRepository,
        ICurrentUser currentUser)
    {
        _subscriptionRepository = subscriptionRepository;
        _digitalServiceRepository = digitalServiceRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> HandleAsync(
        UpdateSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ownerId = _currentUser.UserId;

        var subscription = await _subscriptionRepository.GetByIdAsync(
            command.SubscriptionId,
            ownerId,
            cancellationToken);

        if (subscription is null)
        {
            return false;
        }

        if (command.DigitalServiceId is null)
        {
            subscription.ClearDigitalService();
        }
        else
        {
            if (command.DigitalServiceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Digital service identifier cannot be empty.",
                    nameof(command.DigitalServiceId));
            }

            var digitalService =
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

            subscription.AssignDigitalService(
                digitalService.Id,
                digitalService.Category,
                digitalService.CustomCategoryName,
                digitalService.IconKey,
                digitalService.ManagementUrl);
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
