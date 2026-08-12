using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.SavingsPlans.GetSavingsPlanUsage;
using SubscriptionManager.Domain.Billing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public async Task GetAsync_ShouldReturnDailyUsageForFreePlan()
    {
        var userId =
            Guid.NewGuid();

        await SeedUserAsync(
            _factory.Services,
            userId);

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

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
            3,
            firstUsage.DailyRequestLimit);

        Assert.Equal(
            3,
            firstUsage.RemainingRequestCount);

        Assert.Equal(
            firstUsage,
            secondUsage);
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
                    SubscriptionManager.Infrastructure.Persistence
                        .SubscriptionManagerDbContext>();

        dbContext.Users.Add(
            new SubscriptionManager.Infrastructure.Identity.ApplicationUser
            {
                Id = userId,
                UserName =
                    $"{userId}@example.com",
                Email =
                    $"{userId}@example.com"
            });

        await dbContext.SaveChangesAsync();
    }
}
