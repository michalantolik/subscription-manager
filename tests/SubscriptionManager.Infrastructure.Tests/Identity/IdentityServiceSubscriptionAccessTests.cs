using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Common.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Tests.Identity;

public sealed class IdentityServiceSubscriptionAccessTests
{
    [Theory]
    [InlineData(BillingSubscriptionStatus.Incomplete)]
    [InlineData(BillingSubscriptionStatus.IncompleteExpired)]
    [InlineData(BillingSubscriptionStatus.PastDue)]
    [InlineData(BillingSubscriptionStatus.Canceled)]
    [InlineData(BillingSubscriptionStatus.Unpaid)]
    [InlineData(BillingSubscriptionStatus.Paused)]
    public async Task GetSubscriptionPlanAsync_ShouldReturnFree_WhenStatusDoesNotGrantAccess(
        BillingSubscriptionStatus status)
    {
        await using var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(
                connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionManagerDbContext>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user =
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "user@example.com",
                Email = "user@example.com"
            };

        var createResult =
            await userManager.CreateAsync(
                user);

        Assert.True(
            createResult.Succeeded);

        var periodStart =
            DateTimeOffset.UtcNow.AddDays(-1);

        var periodEnd =
            DateTimeOffset.UtcNow.AddMonths(1);

        var billingSubscription =
            new BillingSubscription(
                Guid.NewGuid(),
                user.Id,
                SubscriptionPlan.Premium,
                BillingInterval.Monthly,
                periodStart,
                periodEnd);

        billingSubscription.Synchronize(
            SubscriptionPlan.Premium,
            BillingInterval.Monthly,
            status,
            "price_premium_monthly",
            periodStart,
            periodEnd,
            false);

        dbContext.BillingSubscriptions.Add(
            billingSubscription);

        await dbContext.SaveChangesAsync();

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.GetSubscriptionPlanAsync(
                user.Id);

        Assert.Equal(
            SubscriptionPlan.Free,
            result);
    }

    private static ServiceProvider CreateServiceProvider(
        SqliteConnection connection)
    {
        var services =
            new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<
            SubscriptionManagerDbContext>(
            options =>
                options.UseSqlite(
                    connection));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<
                SubscriptionManagerDbContext>();

        return services.BuildServiceProvider();
    }
}
