using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Infrastructure.Persistence.Repositories;

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
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscriptions
            .SingleOrDefaultAsync(
                subscription => subscription.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Subscription>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscriptions
            .OrderBy(subscription => subscription.Name)
            .ToListAsync(cancellationToken);
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
