using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class GetSubscriptionsTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetSubscriptionsTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(
            "/api/subscriptions");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnEmptyCollection_WhenNoSubscriptionsExist()
    {
        var response = await _client.GetAsync(
            "/api/subscriptions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var subscriptions = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<SubscriptionResponse>>();

        Assert.NotNull(subscriptions);
        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnOnlySubscriptionsOwnedByCurrentUser()
    {
        var firstUserId =
            Guid.Parse(
                "22222222-2222-2222-2222-222222222222");

        var secondUserId =
            Guid.Parse(
                "33333333-3333-3333-3333-333333333333");

        using var firstUserClient =
            _factory.CreateAuthenticatedClient(firstUserId);

        using var secondUserClient =
            _factory.CreateAuthenticatedClient(secondUserId);

        var createResponse =
            await firstUserClient.PostAsJsonAsync(
                "/api/subscriptions",
                new
                {
                    Name = "Netflix",
                    Amount = 49m,
                    Currency = "PLN",
                    BillingPeriod = BillingPeriod.Monthly,
                    StartDate = new DateOnly(2026, 1, 1)
                });

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var response = await secondUserClient.GetAsync(
            "/api/subscriptions");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var subscriptions = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<SubscriptionResponse>>();

        Assert.NotNull(subscriptions);
        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnSubscriptions_WhenSubscriptionsExist()
    {
        await _client.PostAsJsonAsync(
            "/api/subscriptions",
            new
            {
                Name = "Netflix",
                Amount = 49m,
                Currency = "PLN",
                BillingPeriod = BillingPeriod.Monthly,
                StartDate = new DateOnly(2026, 1, 1)
            });

        await _client.PostAsJsonAsync(
            "/api/subscriptions",
            new
            {
                Name = "Microsoft 365",
                Amount = 299m,
                Currency = "PLN",
                BillingPeriod = BillingPeriod.Yearly,
                StartDate = new DateOnly(2026, 2, 1)
            });

        var response = await _client.GetAsync(
            "/api/subscriptions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var subscriptions = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<SubscriptionResponse>>();

        Assert.NotNull(subscriptions);
        Assert.Equal(2, subscriptions.Count);

        Assert.Contains(
            subscriptions,
            x =>
                x.Name == "Netflix" &&
                x.Amount == 49m &&
                x.Currency == "PLN" &&
                x.BillingPeriod == "Monthly" &&
                x.MonthlyEquivalentAmount == 49m &&
                x.YearlyEquivalentAmount == 588m);

        Assert.Contains(
            subscriptions,
            x =>
                x.Name == "Microsoft 365" &&
                x.Amount == 299m &&
                x.Currency == "PLN" &&
                x.BillingPeriod == "Yearly" &&
                x.MonthlyEquivalentAmount == 299m / 12m &&
                x.YearlyEquivalentAmount == 299m);
    }

    private sealed record SubscriptionResponse(
        Guid Id,
        string Name,
        decimal Amount,
        string Currency,
        string BillingPeriod,
        DateOnly StartDate,
        DateOnly? EndDate,
        bool IsActive,
        decimal MonthlyEquivalentAmount,
        decimal YearlyEquivalentAmount);
}
