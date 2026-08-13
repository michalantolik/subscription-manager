using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Common.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Tests.Persistence;

public sealed class BillingSubscriptionPersistenceTests
{
    [Fact]
    public async Task BillingSubscription_ShouldBeStoredAndMappedBack()
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

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "billing-test@example.com",
            NormalizedUserName = "BILLING-TEST@EXAMPLE.COM",
            Email = "billing-test@example.com",
            NormalizedEmail = "BILLING-TEST@EXAMPLE.COM"
        };

        dbContext.Users.Add(
            user);

        var periodStart =
            new DateTimeOffset(
                2026,
                8,
                9,
                0,
                0,
                0,
                TimeSpan.Zero);

        var periodEnd =
            periodStart.AddMonths(1);

        var subscription =
            new BillingSubscription(
                Guid.NewGuid(),
                user.Id,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                periodStart,
                periodEnd);

        subscription.LinkToPaymentProvider(
            "cus_123",
            "sub_123",
            "price_123");

        dbContext.BillingSubscriptions.Add(
            subscription);

        await dbContext.SaveChangesAsync();

        await using (var command =
                     connection.CreateCommand())
        {
            command.CommandText =
                """
                SELECT
                    Plan,
                    BillingInterval,
                    Status,
                    ProviderCustomerId,
                    ProviderSubscriptionId,
                    ProviderPriceId
                FROM BillingSubscriptions
                """;

            await using var reader =
                await command.ExecuteReaderAsync();

            Assert.True(
                await reader.ReadAsync());

            Assert.Equal(
                "Plus",
                reader.GetString(0));

            Assert.Equal(
                "Monthly",
                reader.GetString(1));

            Assert.Equal(
                "Active",
                reader.GetString(2));

            Assert.Equal(
                "cus_123",
                reader.GetString(3));

            Assert.Equal(
                "sub_123",
                reader.GetString(4));

            Assert.Equal(
                "price_123",
                reader.GetString(5));
        }

        dbContext.ChangeTracker.Clear();

        var loadedSubscription =
            await dbContext.BillingSubscriptions
                .AsNoTracking()
                .SingleAsync(subscription =>
                    subscription.UserId == user.Id);

        Assert.Equal(
            SubscriptionPlan.Plus,
            loadedSubscription.Plan);

        Assert.Equal(
            BillingInterval.Monthly,
            loadedSubscription.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            loadedSubscription.Status);

        Assert.Equal(
            "cus_123",
            loadedSubscription.ProviderCustomerId);

        Assert.Equal(
            "sub_123",
            loadedSubscription.ProviderSubscriptionId);

        Assert.Equal(
            "price_123",
            loadedSubscription.ProviderPriceId);

        Assert.Equal(
            periodStart,
            loadedSubscription.CurrentPeriodStart);

        Assert.Equal(
            periodEnd,
            loadedSubscription.CurrentPeriodEnd);

        Assert.False(
            loadedSubscription.CancelAtPeriodEnd);
    }
}
