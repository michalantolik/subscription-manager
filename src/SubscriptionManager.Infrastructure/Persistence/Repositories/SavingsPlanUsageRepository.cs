using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Domain.SavingsPlans;

namespace SubscriptionManager.Infrastructure.Persistence.Repositories;

internal sealed class SavingsPlanUsageRepository
    : ISavingsPlanUsageRepository
{
    private readonly SubscriptionManagerDbContext _dbContext;

    public SavingsPlanUsageRepository(
        SubscriptionManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GetRemainingRequestCountAsync(
        Guid userId,
        DateOnly usageDateUtc,
        int dailyLimit,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(
            userId,
            usageDateUtc,
            dailyLimit);

        var requestCount =
            await _dbContext.SavingsPlanUsages
                .AsNoTracking()
                .Where(usage =>
                    usage.UserId == userId &&
                    usage.UsageDateUtc == usageDateUtc)
                .Select(usage => usage.RequestCount)
                .SingleOrDefaultAsync(
                    cancellationToken);

        return Math.Max(
            0,
            dailyLimit - requestCount);
    }

    public Task<int?> TryRegisterRequestAsync(
        Guid userId,
        DateOnly usageDateUtc,
        int dailyLimit,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(
            userId,
            usageDateUtc,
            dailyLimit);

        if (!_dbContext.Database.IsRelational())
        {
            return TryRegisterRequestForNonRelationalProviderAsync(
                userId,
                usageDateUtc,
                dailyLimit,
                cancellationToken);
        }

        return TryRegisterRequestForRelationalProviderAsync(
            userId,
            usageDateUtc,
            dailyLimit,
            retryOnInsertConflict: true,
            cancellationToken);
    }

    private async Task<int?>
        TryRegisterRequestForNonRelationalProviderAsync(
            Guid userId,
            DateOnly usageDateUtc,
            int dailyLimit,
            CancellationToken cancellationToken)
    {
        var usage =
            await _dbContext.SavingsPlanUsages
                .SingleOrDefaultAsync(
                    currentUsage =>
                        currentUsage.UserId == userId &&
                        currentUsage.UsageDateUtc == usageDateUtc,
                    cancellationToken);

        if (usage is null)
        {
            usage = new SavingsPlanUsage(
                userId,
                usageDateUtc);

            await _dbContext.SavingsPlanUsages.AddAsync(
                usage,
                cancellationToken);
        }

        if (usage.HasReachedLimit(dailyLimit))
        {
            return null;
        }

        usage.RegisterRequest(dailyLimit);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return usage.GetRemainingRequestCount(
            dailyLimit);
    }

    private async Task<int?>
        TryRegisterRequestForRelationalProviderAsync(
            Guid userId,
            DateOnly usageDateUtc,
            int dailyLimit,
            bool retryOnInsertConflict,
            CancellationToken cancellationToken)
    {
        var updatedRows =
            await _dbContext.SavingsPlanUsages
                .Where(usage =>
                    usage.UserId == userId &&
                    usage.UsageDateUtc == usageDateUtc &&
                    usage.RequestCount < dailyLimit)
                .ExecuteUpdateAsync(
                    setters =>
                        setters.SetProperty(
                            usage => usage.RequestCount,
                            usage => usage.RequestCount + 1),
                    cancellationToken);

        if (updatedRows == 1)
        {
            return await GetRemainingRequestCountAsync(
                userId,
                usageDateUtc,
                dailyLimit,
                cancellationToken);
        }

        var usageExists =
            await _dbContext.SavingsPlanUsages
                .AsNoTracking()
                .AnyAsync(
                    usage =>
                        usage.UserId == userId &&
                        usage.UsageDateUtc == usageDateUtc,
                    cancellationToken);

        if (usageExists)
        {
            return null;
        }

        var usage =
            new SavingsPlanUsage(
                userId,
                usageDateUtc);

        usage.RegisterRequest(
            dailyLimit);

        _dbContext.SavingsPlanUsages.Add(
            usage);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return usage.GetRemainingRequestCount(
                dailyLimit);
        }
        catch (DbUpdateException)
            when (retryOnInsertConflict)
        {
            _dbContext.Entry(usage).State =
                EntityState.Detached;

            return await TryRegisterRequestForRelationalProviderAsync(
                userId,
                usageDateUtc,
                dailyLimit,
                retryOnInsertConflict: false,
                cancellationToken);
        }
    }

    private static void ValidateArguments(
        Guid userId,
        DateOnly usageDateUtc,
        int dailyLimit)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier is required.",
                nameof(userId));
        }

        if (usageDateUtc == default)
        {
            throw new ArgumentException(
                "Usage date is required.",
                nameof(usageDateUtc));
        }

        if (dailyLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyLimit));
        }
    }
}
