using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Infrastructure.Persistence;
using SubscriptionManager.Infrastructure.SavingsPlans;

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
    public async Task TryRegisterRequestAsync_ShouldNotExceedLimit_WhenRequestsAreConcurrent()
    {
        var databasePath =
            Path.Combine(
                Path.GetTempPath(),
                $"subscription-manager-{Guid.NewGuid():N}.db");

        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                DefaultTimeout = 30
            }.ToString();

        var options =
            new DbContextOptionsBuilder<
                    SubscriptionManagerDbContext>()
                .UseSqlite(connectionString)
                .Options;

        try
        {
            await using (var setupDbContext =
                         new SubscriptionManagerDbContext(
                             options))
            {
                await setupDbContext.Database
                    .EnsureCreatedAsync();
            }

            var userId = Guid.NewGuid();
            var usageDateUtc =
                new DateOnly(2026, 8, 7);
            const int dailyLimit = 3;
            const int attemptedRequestCount = 10;

            var tasks =
                Enumerable
                    .Range(
                        0,
                        attemptedRequestCount)
                    .Select(
                        _ => RegisterRequestAsync(
                            options,
                            userId,
                            usageDateUtc,
                            dailyLimit));

            var results =
                await Task.WhenAll(tasks);

            Assert.Equal(
                dailyLimit,
                results.Count(
                    result => result.HasValue));

            Assert.Equal(
                attemptedRequestCount - dailyLimit,
                results.Count(
                    result => result is null));

            await using var verificationDbContext =
                new SubscriptionManagerDbContext(
                    options);

            var usage =
                await verificationDbContext
                    .SavingsPlanUsages
                    .AsNoTracking()
                    .SingleAsync();

            Assert.Equal(
                dailyLimit,
                usage.RequestCount);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
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

        Assert.Equal(
            3,
            usages.Count);

        Assert.All(
            usages,
            usage => Assert.Equal(
                1,
                usage.RequestCount));
    }

    private static async Task<int?> RegisterRequestAsync(
        DbContextOptions<SubscriptionManagerDbContext> options,
        Guid userId,
        DateOnly usageDateUtc,
        int dailyLimit)
    {
        await using var dbContext =
            new SubscriptionManagerDbContext(
                options);

        var repository =
            new SavingsPlanUsageRepository(
                dbContext);

        return await repository.TryRegisterRequestAsync(
            userId,
            usageDateUtc,
            dailyLimit);
    }
}
