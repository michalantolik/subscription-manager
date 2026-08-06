using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Infrastructure.Persistence;
using SubscriptionManager.Infrastructure.Persistence.Repositories;

namespace SubscriptionManager.Infrastructure.Tests.Persistence;

public sealed class SavingsPlanUsageRepositoryTests
{
    [Fact]
    public async Task TryRegisterRequestAsync_ShouldRegisterRequestsUpToDailyLimit()
    {
        await using var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<
                    SubscriptionManagerDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var dbContext =
            new SubscriptionManagerDbContext(
                options);

        await dbContext.Database.EnsureCreatedAsync();

        var repository =
            new SavingsPlanUsageRepository(
                dbContext);

        var userId = Guid.NewGuid();
        var usageDateUtc =
            new DateOnly(2026, 8, 6);

        var firstResult =
            await repository.TryRegisterRequestAsync(
                userId,
                usageDateUtc,
                dailyLimit: 3);

        var secondResult =
            await repository.TryRegisterRequestAsync(
                userId,
                usageDateUtc,
                dailyLimit: 3);

        var thirdResult =
            await repository.TryRegisterRequestAsync(
                userId,
                usageDateUtc,
                dailyLimit: 3);

        var fourthResult =
            await repository.TryRegisterRequestAsync(
                userId,
                usageDateUtc,
                dailyLimit: 3);

        Assert.Equal(2, firstResult);
        Assert.Equal(1, secondResult);
        Assert.Equal(0, thirdResult);
        Assert.Null(fourthResult);

        var usage =
            await dbContext.SavingsPlanUsages
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(userId, usage.UserId);
        Assert.Equal(usageDateUtc, usage.UsageDateUtc);
        Assert.Equal(3, usage.RequestCount);
    }

    [Fact]
    public async Task TryRegisterRequestAsync_ShouldUseSeparateCountersForUsersAndDates()
    {
        await using var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<
                    SubscriptionManagerDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var dbContext =
            new SubscriptionManagerDbContext(
                options);

        await dbContext.Database.EnsureCreatedAsync();

        var repository =
            new SavingsPlanUsageRepository(
                dbContext);

        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();
        var firstDate =
            new DateOnly(2026, 8, 6);
        var secondDate =
            new DateOnly(2026, 8, 7);

        await repository.TryRegisterRequestAsync(
            firstUserId,
            firstDate,
            dailyLimit: 3);

        await repository.TryRegisterRequestAsync(
            firstUserId,
            secondDate,
            dailyLimit: 3);

        await repository.TryRegisterRequestAsync(
            secondUserId,
            firstDate,
            dailyLimit: 3);

        var usages =
            await dbContext.SavingsPlanUsages
                .AsNoTracking()
                .ToListAsync();

        Assert.Equal(3, usages.Count);

        Assert.All(
            usages,
            usage => Assert.Equal(
                1,
                usage.RequestCount));
    }
}
