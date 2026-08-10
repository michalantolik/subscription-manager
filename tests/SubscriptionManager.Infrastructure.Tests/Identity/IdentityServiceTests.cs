using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.SavingsPlans;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Tests.Identity;

public sealed class IdentityServiceTests
{
    [Fact]
    public async Task GetBaseCurrencyAsync_ShouldReturnUserBaseCurrency_WhenUserExists()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com",
            BaseCurrency = Currency.EUR
        };

        var createResult =
            await userManager.CreateAsync(user);

        Assert.True(createResult.Succeeded);

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.GetBaseCurrencyAsync(
                user.Id);

        Assert.Equal(
            Currency.EUR,
            result);
    }

    [Fact]
    public async Task GetBaseCurrencyAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

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
            await identityService.GetBaseCurrencyAsync(
                Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSubscriptionPlanAsync_ShouldReturnFree_WhenUserHasNoBillingSubscription()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com"
        };

        var createResult =
            await userManager.CreateAsync(user);

        Assert.True(createResult.Succeeded);

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

    [Fact]
    public async Task GetSubscriptionPlanAsync_ShouldReturnPaidPlan_WhenBillingSubscriptionIsActive()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com"
        };

        var createResult =
            await userManager.CreateAsync(user);

        Assert.True(createResult.Succeeded);

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
            SubscriptionPlan.Plus,
            result);
    }

    [Fact]
    public async Task GetSubscriptionPlanAsync_ShouldReturnFree_WhenBillingSubscriptionHasExpired()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com"
        };

        var createResult =
            await userManager.CreateAsync(user);

        Assert.True(createResult.Succeeded);

        var periodStart =
            DateTimeOffset.UtcNow.AddMonths(-1);

        var periodEnd =
            DateTimeOffset.UtcNow.AddDays(-1);

        var billingSubscription =
            new BillingSubscription(
                Guid.NewGuid(),
                user.Id,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                periodStart,
                periodEnd);

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

    [Fact]
    public async Task GetSubscriptionPlanAsync_ShouldReturnPaidPlan_WhenCancellationIsScheduled()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com"
        };

        var createResult =
            await userManager.CreateAsync(user);

        Assert.True(createResult.Succeeded);

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

        billingSubscription.ScheduleCancellation();

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
            SubscriptionPlan.Plus,
            result);
    }

    [Fact]
    public async Task GetSubscriptionPlanAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

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
            await identityService.GetSubscriptionPlanAsync(
                Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAccountPreferencesAsync_ShouldUpdatePreferences_WhenUserExists()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com",
            Language = Language.Polish,
            BaseCurrency = Currency.PLN
        };

        var createResult =
            await userManager.CreateAsync(user);

        Assert.True(createResult.Succeeded);

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.UpdateAccountPreferencesAsync(
                user.Id,
                Language.German,
                Currency.EUR);

        Assert.True(result);

        var preferences =
            await dbContext.Users
                .AsNoTracking()
                .Where(currentUser =>
                    currentUser.Id == user.Id)
                .Select(currentUser =>
                    new
                    {
                        currentUser.Language,
                        currentUser.BaseCurrency
                    })
                .SingleAsync();

        Assert.Equal(
            Language.German,
            preferences.Language);

        Assert.Equal(
            Currency.EUR,
            preferences.BaseCurrency);
    }

    [Fact]
    public async Task UpdateAccountPreferencesAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

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
            await identityService.UpdateAccountPreferencesAsync(
                Guid.NewGuid(),
                Language.English,
                Currency.EUR);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAccountPreferencesAsync_ShouldThrow_WhenLanguageIsInvalid()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

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

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            identityService.UpdateAccountPreferencesAsync(
                Guid.NewGuid(),
                (Language)999,
                Currency.EUR));
    }

    [Fact]
    public async Task UpdateAccountPreferencesAsync_ShouldThrow_WhenBaseCurrencyIsInvalid()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

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

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            identityService.UpdateAccountPreferencesAsync(
                Guid.NewGuid(),
                Language.English,
                (Currency)999));
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUserAndOwnedData()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

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
            Email = "user@example.com",
            BaseCurrency = Currency.PLN
        };

        var otherUser = new ApplicationUser
        {
            Id = otherUserId,
            UserName = "other@example.com",
            Email = "other@example.com",
            BaseCurrency = Currency.EUR
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
                Currency.PLN,
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
                Currency.PLN,
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

        var savingsPlanUsage =
            new SavingsPlanUsage(
                userId,
                new DateOnly(2026, 8, 6));

        savingsPlanUsage.RegisterRequest(
            SubscriptionPlanLimits.PlusDailySavingsPlanLimit);

        dbContext.SavingsPlanUsages.Add(
            savingsPlanUsage);

        await dbContext.SaveChangesAsync();

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.DeleteUserAsync(
                userId);

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

        Assert.False(
            await dbContext.SavingsPlanUsages
                .AsNoTracking()
                .AnyAsync(usage =>
                    usage.UserId == userId));

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
                    digitalService.Id ==
                    predefinedService.Id));

        Assert.True(
            await dbContext.DigitalServices
                .AsNoTracking()
                .AnyAsync(digitalService =>
                    digitalService.Id ==
                    otherUserCustomService.Id));
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

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

        Assert.Equal(
            "UserNotFound",
            error.Code);

        Assert.Equal(
            "The user was not found.",
            error.Description);
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldLockUserOut_AfterMaximumFailedAttempts()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true
        };

        var createResult =
            await userManager.CreateAsync(
                user,
                "Password123!");

        Assert.True(createResult.Succeeded);

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var result =
                await identityService.AuthenticateUserAsync(
                    user.Email!,
                    "WrongPassword123!");

            Assert.False(result.Succeeded);
        }

        var storedUser =
            await userManager.FindByIdAsync(
                user.Id.ToString());

        Assert.NotNull(storedUser);

        Assert.True(
            await userManager.IsLockedOutAsync(
                storedUser));
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldReturnInvalidCredentials_WhenEmailIsNotConfirmedAndPasswordIsInvalid()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = false
        };

        var createResult =
            await userManager.CreateAsync(
                user,
                "Password123!");

        Assert.True(createResult.Succeeded);

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.AuthenticateUserAsync(
                user.Email!,
                "WrongPassword123!");

        Assert.False(result.Succeeded);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "InvalidCredentials",
            error.Code);

        Assert.Equal(
            "The email address or password is invalid.",
            error.Description);
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldReturnPaidPlan_FromBillingSubscription()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var serviceProvider =
            CreateServiceProvider(connection);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true
        };

        var createResult =
            await userManager.CreateAsync(
                user,
                "Password123!");

        Assert.True(createResult.Succeeded);

        var billingSubscription =
            new BillingSubscription(
                Guid.NewGuid(),
                user.Id,
                SubscriptionPlan.Premium,
                BillingInterval.Monthly,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddMonths(1));

        dbContext.BillingSubscriptions.Add(
            billingSubscription);

        await dbContext.SaveChangesAsync();

        var identityService =
            new IdentityService(
                userManager,
                dbContext);

        var result =
            await identityService.AuthenticateUserAsync(
                user.Email!,
                "Password123!");

        Assert.True(result.Succeeded);

        Assert.Equal(
            SubscriptionPlan.Premium,
            result.SubscriptionPlan);
    }

    private static ServiceProvider CreateServiceProvider(
        SqliteConnection connection)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<SubscriptionManagerDbContext>(
            options =>
                options.UseSqlite(connection));

        services
            .AddIdentityCore<ApplicationUser>(
                options =>
                {
                    options.Lockout.AllowedForNewUsers = true;

                    options.Lockout.MaxFailedAccessAttempts = 5;

                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(5);
                })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<SubscriptionManagerDbContext>();

        return services.BuildServiceProvider();
    }
}
