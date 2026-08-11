using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Tests.Identity;

public sealed class IdentityServiceAccountDeletionTests
{
    [Fact]
    public async Task DeleteUserAsync_ShouldNotDeleteUser_WhenProviderSubscriptionIsActive()
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
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                periodStart,
                periodEnd);

        billingSubscription.LinkToPaymentProvider(
            "cus_123",
            "sub_123",
            "price_plus_monthly");

        dbContext.BillingSubscriptions.Add(
            billingSubscription);

        await dbContext.SaveChangesAsync();

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.DeleteUserAsync(
                user.Id);

        Assert.False(
            result.Succeeded);

        var error =
            Assert.Single(
                result.Errors);

        Assert.Equal(
            "BillingSubscriptionActive",
            error.Code);

        Assert.Equal(
            "The billing subscription must end before the account can be deleted.",
            error.Description);

        Assert.True(
            await dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    currentUser =>
                        currentUser.Id == user.Id));

        Assert.True(
            await dbContext.BillingSubscriptions
                .AsNoTracking()
                .AnyAsync(
                    subscription =>
                        subscription.UserId == user.Id));
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
