using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices;

public interface IDigitalServiceRepository
{
    Task<IReadOnlyCollection<DigitalService>> GetAvailableAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<DigitalService?> GetAvailableByIdAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken = default);
}
