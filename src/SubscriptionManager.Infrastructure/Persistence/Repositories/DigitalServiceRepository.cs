using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Infrastructure.Persistence.Repositories;

internal sealed class DigitalServiceRepository : IDigitalServiceRepository
{
    private readonly SubscriptionManagerDbContext _dbContext;

    public DigitalServiceRepository(SubscriptionManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        DigitalService digitalService,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.DigitalServices.AddAsync(
            digitalService,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<DigitalService>> GetAvailableAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DigitalServices
            .AsNoTracking()
            .Where(x => x.IsActive && (x.IsPredefined || x.OwnerId == ownerId))
            .OrderByDescending(x => x.IsPredefined)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<DigitalService?> GetAvailableByIdAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DigitalServices
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id && x.IsActive &&
                    (x.IsPredefined || x.OwnerId == ownerId),
                cancellationToken);
    }

    public async Task<DigitalService?> GetCustomByIdAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DigitalServices.SingleOrDefaultAsync(
            x => x.Id == id && !x.IsPredefined && x.OwnerId == ownerId,
            cancellationToken);
    }

    public void Remove(DigitalService digitalService)
    {
        _dbContext.DigitalServices.Remove(digitalService);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
