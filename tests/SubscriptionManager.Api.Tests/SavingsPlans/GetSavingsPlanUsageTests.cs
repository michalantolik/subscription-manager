using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Api.Tests.Authentication;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.SavingsPlans.GetSavingsPlanUsage;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Api.Tests.SavingsPlans;

public sealed class GetSavingsPlanUsageTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    private readonly CustomWebApplicationFactory _factory;

    public GetSavingsPlanUsageTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response =
            await client.GetAsync(
                "/api/savings-plans/usage");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCurrentUsageWithoutRegisteringRequest()
    {
        var userId =
            Guid.NewGuid();

        await SeedUserAsync(
            _factory.Services,
            userId);

        await SeedUsageAsync(
            _factory.Services,
            userId,
            requestCount: 1);

        using var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            userId.ToString());

        var firstResponse =
            await client.GetAsync(
                "/api/savings-plans/usage");

        var secondResponse =
            await client.GetAsync(
                "/api/savings-plans/usage");

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.OK,
            secondResponse.StatusCode);

        var firstUsage =
            await firstResponse.Content
                .ReadFromJsonAsync<SavingsPlanUsageDto>(
                    JsonOptions);

        var secondUsage =
            await secondResponse.Content
                .ReadFromJsonAsync<SavingsPlanUsageDto>(
                    JsonOptions);

        Assert.NotNull(firstUsage);
        Assert.NotNull(secondUsage);

        Assert.Equal(
            SubscriptionPlan.Free,
            firstUsage.SubscriptionPlan);

        Assert.Equal(
            SubscriptionPlanLimits
                .FreeDailySavingsPlanLimit,
            firstUsage.DailyRequestLimit);

        Assert.Equal(
            SubscriptionPlanLimits
                .FreeDailySavingsPlanLimit - 1,
            firstUsage.RemainingRequestCount);

        Assert.Equal(
            firstUsage,
            secondUsage);

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionManagerDbContext>();

        var storedUsage =
            await dbContext.SavingsPlanUsages
                .FindAsync(
                    userId,
                    DateOnly.FromDateTime(
                        DateTime.UtcNow));

        Assert.NotNull(storedUsage);

        Assert.Equal(
            1,
            storedUsage.RequestCount);
    }

    private static async Task SeedUserAsync(
        IServiceProvider services,
        Guid userId)
    {
        await using var scope =
            services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionManagerDbContext>();

        dbContext.Users.Add(
            new ApplicationUser
            {
                Id = userId,
                UserName =
                    $"{userId}@example.com",
                Email =
                    $"{userId}@example.com",
                SubscriptionPlan =
                    SubscriptionPlan.Free
            });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsageAsync(
        IServiceProvider services,
        Guid userId,
        int requestCount)
    {
        await using var scope =
            services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionManagerDbContext>();

        var usage =
            new Domain.SavingsPlans.SavingsPlanUsage(
                userId,
                DateOnly.FromDateTime(
                    DateTime.UtcNow));

        for (var index = 0;
             index < requestCount;
             index++)
        {
            usage.RegisterRequest(
                SubscriptionPlanLimits
                    .FreeDailySavingsPlanLimit);
        }

        dbContext.SavingsPlanUsages.Add(
            usage);

        await dbContext.SaveChangesAsync();
    }
}
