using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.Billing.ProcessWebhook;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Billing.Persistence;
using SubscriptionManager.Infrastructure.Common.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Tests.Persistence;

public sealed class BillingWebhookRepositoryTests
{
    [Fact]
    public async Task ApplyAsync_ShouldCreateSubscriptionAndRecordEvent()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var userId =
            Guid.NewGuid();

        await AddUserAsync(
            database.DbContext,
            userId);

        var eventCreatedAt =
            new DateTimeOffset(
                2026,
                8,
                10,
                10,
                0,
                0,
                TimeSpan.Zero);

        var paymentEvent =
            CreatePaymentEvent(
                "evt_123",
                userId,
                eventCreatedAt,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                BillingSubscriptionStatus.Active,
                "price_plus_monthly");

        var repository =
            new BillingWebhookRepository(
                database.DbContext);

        var result =
            await repository.ApplyAsync(
                paymentEvent,
                eventCreatedAt.AddSeconds(1));

        Assert.Equal(
            PaymentWebhookProcessingResult.Applied,
            result);

        database.DbContext.ChangeTracker.Clear();

        var subscription =
            await database.DbContext
                .BillingSubscriptions
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(
            userId,
            subscription.UserId);

        Assert.Equal(
            SubscriptionPlan.Plus,
            subscription.Plan);

        Assert.Equal(
            BillingInterval.Monthly,
            subscription.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            subscription.Status);

        Assert.Equal(
            "cus_123",
            subscription.ProviderCustomerId);

        Assert.Equal(
            "sub_123",
            subscription.ProviderSubscriptionId);

        Assert.Equal(
            "price_plus_monthly",
            subscription.ProviderPriceId);

        Assert.Equal(
            eventCreatedAt,
            subscription.LastProviderEventCreatedAt);

        Assert.Equal(
            1,
            await database.DbContext
                .ProcessedBillingEvents
                .CountAsync());
    }

    [Fact]
    public async Task ApplyAsync_WithProcessedEvent_ShouldReturnDuplicate()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var userId =
            Guid.NewGuid();

        await AddUserAsync(
            database.DbContext,
            userId);

        var eventCreatedAt =
            new DateTimeOffset(
                2026,
                8,
                10,
                10,
                0,
                0,
                TimeSpan.Zero);

        var paymentEvent =
            CreatePaymentEvent(
                "evt_duplicate",
                userId,
                eventCreatedAt,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                BillingSubscriptionStatus.Active,
                "price_plus_monthly");

        var repository =
            new BillingWebhookRepository(
                database.DbContext);

        var firstResult =
            await repository.ApplyAsync(
                paymentEvent,
                eventCreatedAt.AddSeconds(1));

        var secondResult =
            await repository.ApplyAsync(
                paymentEvent,
                eventCreatedAt.AddSeconds(2));

        Assert.Equal(
            PaymentWebhookProcessingResult.Applied,
            firstResult);

        Assert.Equal(
            PaymentWebhookProcessingResult.Duplicate,
            secondResult);

        Assert.Equal(
            1,
            await database.DbContext
                .BillingSubscriptions
                .CountAsync());

        Assert.Equal(
            1,
            await database.DbContext
                .ProcessedBillingEvents
                .CountAsync());
    }

    [Fact]
    public async Task ApplyAsync_WithOlderEvent_ShouldKeepNewerState()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var userId =
            Guid.NewGuid();

        await AddUserAsync(
            database.DbContext,
            userId);

        var newerEventCreatedAt =
            new DateTimeOffset(
                2026,
                9,
                10,
                10,
                0,
                0,
                TimeSpan.Zero);

        var olderEventCreatedAt =
            newerEventCreatedAt.AddDays(-1);

        var repository =
            new BillingWebhookRepository(
                database.DbContext);

        var newerEvent =
            CreatePaymentEvent(
                "evt_newer",
                userId,
                newerEventCreatedAt,
                SubscriptionPlan.Premium,
                BillingInterval.Yearly,
                BillingSubscriptionStatus.Active,
                "price_premium_yearly");

        var olderEvent =
            CreatePaymentEvent(
                "evt_older",
                userId,
                olderEventCreatedAt,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                BillingSubscriptionStatus.PastDue,
                "price_plus_monthly");

        var newerResult =
            await repository.ApplyAsync(
                newerEvent,
                newerEventCreatedAt.AddSeconds(1));

        var olderResult =
            await repository.ApplyAsync(
                olderEvent,
                newerEventCreatedAt.AddSeconds(2));

        Assert.Equal(
            PaymentWebhookProcessingResult.Applied,
            newerResult);

        Assert.Equal(
            PaymentWebhookProcessingResult.Stale,
            olderResult);

        database.DbContext.ChangeTracker.Clear();

        var subscription =
            await database.DbContext
                .BillingSubscriptions
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(
            SubscriptionPlan.Premium,
            subscription.Plan);

        Assert.Equal(
            BillingInterval.Yearly,
            subscription.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            subscription.Status);

        Assert.Equal(
            "price_premium_yearly",
            subscription.ProviderPriceId);

        Assert.Equal(
            newerEventCreatedAt,
            subscription.LastProviderEventCreatedAt);

        Assert.Equal(
            2,
            await database.DbContext
                .ProcessedBillingEvents
                .CountAsync());
    }

    [Fact]
    public async Task ApplyAsync_WhenSubscriptionCannotBeSaved_ShouldNotRecordEvent()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var eventCreatedAt =
            new DateTimeOffset(
                2026,
                8,
                10,
                10,
                0,
                0,
                TimeSpan.Zero);

        var paymentEvent =
            CreatePaymentEvent(
                "evt_failed",
                Guid.NewGuid(),
                eventCreatedAt,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                BillingSubscriptionStatus.Active,
                "price_plus_monthly");

        var repository =
            new BillingWebhookRepository(
                database.DbContext);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repository.ApplyAsync(
                paymentEvent,
                eventCreatedAt.AddSeconds(1)));

        database.DbContext.ChangeTracker.Clear();

        Assert.Equal(
            0,
            await database.DbContext
                .BillingSubscriptions
                .CountAsync());

        Assert.Equal(
            0,
            await database.DbContext
                .ProcessedBillingEvents
                .CountAsync());
    }

    private static PaymentSubscriptionEvent CreatePaymentEvent(
        string providerEventId,
        Guid userId,
        DateTimeOffset eventCreatedAt,
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        BillingSubscriptionStatus status,
        string priceId)
    {
        var periodStart =
            eventCreatedAt.Date;

        var periodEnd =
            billingInterval == BillingInterval.Monthly
                ? periodStart.AddMonths(1)
                : periodStart.AddYears(1);

        return new PaymentSubscriptionEvent(
            providerEventId,
            eventCreatedAt,
            userId,
            "cus_123",
            "sub_123",
            priceId,
            plan,
            billingInterval,
            status,
            periodStart,
            periodEnd,
            false);
    }

    private static async Task AddUserAsync(
        SubscriptionManagerDbContext dbContext,
        Guid userId)
    {
        dbContext.Users.Add(
            new ApplicationUser
            {
                Id = userId,
                UserName =
                    $"{userId}@example.com",
                NormalizedUserName =
                    $"{userId}@example.com".ToUpperInvariant(),
                Email =
                    $"{userId}@example.com",
                NormalizedEmail =
                    $"{userId}@example.com".ToUpperInvariant()
            });

        await dbContext.SaveChangesAsync();
    }

    private sealed class TestDatabase
        : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(
            SqliteConnection connection,
            SubscriptionManagerDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public SubscriptionManagerDbContext DbContext { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection =
                new SqliteConnection(
                    "Data Source=:memory:");

            await connection.OpenAsync();

            var options =
                new DbContextOptionsBuilder<
                        SubscriptionManagerDbContext>()
                    .UseSqlite(
                        connection)
                    .Options;

            var dbContext =
                new SubscriptionManagerDbContext(
                    options);

            await dbContext.Database
                .EnsureCreatedAsync();

            return new TestDatabase(
                connection,
                dbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
