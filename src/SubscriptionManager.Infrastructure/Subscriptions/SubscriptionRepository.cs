using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Subscriptions;

/// <summary>
/// Provides persistence for subscriptions.
/// </summary>
internal sealed class SubscriptionRepository
    : ISubscriptionRepository
{
    private readonly SubscriptionManagerDbContext _dbContext;

    public SubscriptionRepository(
        SubscriptionManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Subscription subscription,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Subscriptions.AddAsync(
            subscription,
            cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<Subscription> subscriptions,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Subscriptions.AddRangeAsync(
            subscriptions,
            cancellationToken);
    }

    public async Task<Subscription?> GetByIdAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscriptions
            .SingleOrDefaultAsync(
                subscription =>
                    subscription.Id == id &&
                    subscription.OwnerId == ownerId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Subscription>> GetAllAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscriptions
            .AsNoTracking()
            .Where(subscription =>
                subscription.OwnerId == ownerId)
            .OrderBy(subscription => subscription.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveCountAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscriptions
            .AsNoTracking()
            .CountAsync(
                subscription =>
                    subscription.OwnerId == ownerId &&
                    subscription.EndDate == null,
                cancellationToken);
    }

    public void Remove(
        Subscription subscription)
    {
        _dbContext.Subscriptions.Remove(subscription);
    }

    public void RemoveRange(
        IEnumerable<Subscription> subscriptions)
    {
        _dbContext.Subscriptions.RemoveRange(subscriptions);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
