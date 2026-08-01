using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Tests.Identity;

public sealed class IdentityServiceTests
{
    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUserAndOwnedData()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<SubscriptionManagerDbContext>(
            options => options.UseSqlite(connection));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<SubscriptionManagerDbContext>();

        await using var serviceProvider =
            services.BuildServiceProvider();

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "user@example.com",
            Email = "user@example.com"
        };

        var otherUser = new ApplicationUser
        {
            Id = otherUserId,
            UserName = "other@example.com",
            Email = "other@example.com"
        };

        var createUserResult =
            await userManager.CreateAsync(user);

        var createOtherUserResult =
            await userManager.CreateAsync(otherUser);

        Assert.True(createUserResult.Succeeded);
        Assert.True(createOtherUserResult.Succeeded);

        var createdAt = DateTimeOffset.UtcNow;

        var predefinedService =
            DigitalService.CreatePredefined(
                Guid.NewGuid(),
                "netflix",
                "Netflix",
                DigitalServiceCategory.Video,
                "netflix",
                "https://www.netflix.com/account",
                10,
                createdAt);

        var customService =
            DigitalService.CreateCustom(
                Guid.NewGuid(),
                userId,
                "custom-service",
                "Custom service",
                DigitalServiceCategory.Other,
                "Custom",
                null,
                null,
                createdAt);

        var otherUserCustomService =
            DigitalService.CreateCustom(
                Guid.NewGuid(),
                otherUserId,
                "other-service",
                "Other service",
                DigitalServiceCategory.Other,
                "Other",
                null,
                null,
                createdAt);

        var subscription =
            new Subscription(
                Guid.NewGuid(),
                userId,
                "User subscription",
                29.99m,
                "PLN",
                BillingPeriod.Monthly,
                DateOnly.FromDateTime(DateTime.UtcNow));

        subscription.AssignDigitalService(
            customService.Id,
            customService.Category,
            customService.CustomCategoryName,
            customService.IconKey,
            customService.ManagementUrl);

        var otherUserSubscription =
            new Subscription(
                Guid.NewGuid(),
                otherUserId,
                "Other user subscription",
                49.99m,
                "PLN",
                BillingPeriod.Monthly,
                DateOnly.FromDateTime(DateTime.UtcNow));

        otherUserSubscription.AssignDigitalService(
            otherUserCustomService.Id,
            otherUserCustomService.Category,
            otherUserCustomService.CustomCategoryName,
            otherUserCustomService.IconKey,
            otherUserCustomService.ManagementUrl);

        dbContext.DigitalServices.AddRange(
            predefinedService,
            customService,
            otherUserCustomService);

        dbContext.Subscriptions.AddRange(
            subscription,
            otherUserSubscription);

        await dbContext.SaveChangesAsync();

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.DeleteUserAsync(userId);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);

        Assert.False(
            await dbContext.Users
                .AsNoTracking()
                .AnyAsync(currentUser =>
                    currentUser.Id == userId));

        Assert.True(
            await dbContext.Users
                .AsNoTracking()
                .AnyAsync(currentUser =>
                    currentUser.Id == otherUserId));

        Assert.False(
            await dbContext.Subscriptions
                .AsNoTracking()
                .AnyAsync(currentSubscription =>
                    currentSubscription.OwnerId == userId));

        Assert.True(
            await dbContext.Subscriptions
                .AsNoTracking()
                .AnyAsync(currentSubscription =>
                    currentSubscription.OwnerId == otherUserId));

        Assert.False(
            await dbContext.DigitalServices
                .AsNoTracking()
                .AnyAsync(digitalService =>
                    digitalService.OwnerId == userId));

        Assert.True(
            await dbContext.DigitalServices
                .AsNoTracking()
                .AnyAsync(digitalService =>
                    digitalService.Id == predefinedService.Id));

        Assert.True(
            await dbContext.DigitalServices
                .AsNoTracking()
                .AnyAsync(digitalService =>
                    digitalService.Id == otherUserCustomService.Id));
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<SubscriptionManagerDbContext>(
            options => options.UseSqlite(connection));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<SubscriptionManagerDbContext>();

        await using var serviceProvider =
            services.BuildServiceProvider();

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.DeleteUserAsync(
                Guid.NewGuid());

        Assert.False(result.Succeeded);

        var error = Assert.Single(result.Errors);

        Assert.Equal("UserNotFound", error.Code);
        Assert.Equal(
            "The user was not found.",
            error.Description);
    }
}
