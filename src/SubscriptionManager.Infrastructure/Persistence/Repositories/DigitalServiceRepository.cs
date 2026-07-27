using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Infrastructure.Persistence.Repositories;

internal sealed class DigitalServiceRepository
    : IDigitalServiceRepository
{
    private readonly SubscriptionManagerDbContext _dbContext;

    public DigitalServiceRepository(
        SubscriptionManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<DigitalService>> GetAvailableAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DigitalServices
            .AsNoTracking()
            .Where(digitalService =>
                digitalService.IsActive &&
                (digitalService.IsPredefined ||
                 digitalService.OwnerId == ownerId))
            .OrderByDescending(digitalService =>
                digitalService.IsPredefined)
            .ThenBy(digitalService =>
                digitalService.SortOrder)
            .ThenBy(digitalService =>
                digitalService.Name)
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
                digitalService =>
                    digitalService.Id == id &&
                    digitalService.IsActive &&
                    (digitalService.IsPredefined ||
                     digitalService.OwnerId == ownerId),
                cancellationToken);
    }
}
