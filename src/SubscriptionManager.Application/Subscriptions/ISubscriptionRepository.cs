using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions;

public interface ISubscriptionRepository
{
    Task AddAsync(
        Subscription subscription,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<Subscription> subscriptions,
        CancellationToken cancellationToken = default);

    Task<Subscription?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Subscription>> GetAllAsync(
        CancellationToken cancellationToken = default);

    void Remove(
        Subscription subscription);

    void RemoveRange(
        IEnumerable<Subscription> subscriptions);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}