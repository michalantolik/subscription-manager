using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices;

/// <summary>
/// Persistence operations for digital service use cases.
/// </summary>
public interface IDigitalServiceRepository
{
    Task AddAsync(
        DigitalService digitalService,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DigitalService>> GetAvailableAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<DigitalService?> GetAvailableByIdAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<DigitalService?> GetCustomByIdAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    void Remove(DigitalService digitalService);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
