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

    public async Task<IReadOnlyCollection<DigitalService>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DigitalServices
            .AsNoTracking()
            .Where(digitalService =>
                digitalService.IsPredefined &&
                digitalService.IsActive)
            .OrderBy(digitalService =>
                digitalService.SortOrder)
            .ThenBy(digitalService =>
                digitalService.Name)
            .ToListAsync(cancellationToken);
    }
}
