using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices;

public interface IDigitalServiceRepository
{
    Task<IReadOnlyCollection<DigitalService>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
