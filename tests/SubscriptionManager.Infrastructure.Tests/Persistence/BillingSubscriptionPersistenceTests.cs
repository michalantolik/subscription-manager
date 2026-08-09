using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Identity;
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

        dbContext.Users.Add(user);

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

        dbContext.BillingSubscriptions.Add(
            subscription);

        await dbContext.SaveChangesAsync();

        await using (var command =
                     connection.CreateCommand())
        {
            command.CommandText =
                """
                SELECT Plan, BillingInterval, Status
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
        }

        dbContext.ChangeTracker.Clear();

        var loadedSubscription =
            await dbContext.BillingSubscriptions
                .AsNoTracking()
                .SingleAsync(x =>
                    x.UserId == user.Id);

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
            periodStart,
            loadedSubscription.CurrentPeriodStart);

        Assert.Equal(
            periodEnd,
            loadedSubscription.CurrentPeriodEnd);

        Assert.False(
            loadedSubscription.CancelAtPeriodEnd);
    }
}
