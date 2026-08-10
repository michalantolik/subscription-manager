using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class GetSubscriptionsTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public GetSubscriptionsTests(
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
                "/api/subscriptions");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnEmptyCollection_WhenNoSubscriptionsExist()
    {
        var userId =
            Guid.NewGuid();

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var response =
            await client.GetAsync(
                "/api/subscriptions");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var subscriptions =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyCollection<SubscriptionResponse>>();

        Assert.NotNull(subscriptions);
        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnOnlySubscriptionsOwnedByCurrentUser()
    {
        var firstUserId =
            Guid.NewGuid();

        var secondUserId =
            Guid.NewGuid();

        using var firstUserClient =
            _factory.CreateAuthenticatedClient(
                firstUserId);

        using var secondUserClient =
            _factory.CreateAuthenticatedClient(
                secondUserId);

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

        var response =
            await secondUserClient.GetAsync(
                "/api/subscriptions");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var subscriptions =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyCollection<SubscriptionResponse>>();

        Assert.NotNull(subscriptions);
        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnSubscriptions_WhenSubscriptionsExist()
    {
        var userId =
            Guid.NewGuid();

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var firstCreateResponse =
            await client.PostAsJsonAsync(
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
            firstCreateResponse.StatusCode);

        var secondCreateResponse =
            await client.PostAsJsonAsync(
                "/api/subscriptions",
                new
                {
                    Name = "Microsoft 365",
                    Amount = 299m,
                    Currency = "PLN",
                    BillingPeriod = BillingPeriod.Yearly,
                    StartDate = new DateOnly(2026, 2, 1)
                });

        Assert.Equal(
            HttpStatusCode.Created,
            secondCreateResponse.StatusCode);

        var response =
            await client.GetAsync(
                "/api/subscriptions");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var subscriptions =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyCollection<SubscriptionResponse>>();

        Assert.NotNull(subscriptions);

        Assert.Equal(
            2,
            subscriptions.Count);

        Assert.Contains(
            subscriptions,
            subscription =>
                subscription.Name == "Netflix" &&
                subscription.Amount == 49m &&
                subscription.Currency == "PLN" &&
                subscription.BillingPeriod == "Monthly" &&
                subscription.MonthlyEquivalentAmount == 49m &&
                subscription.YearlyEquivalentAmount == 588m);

        Assert.Contains(
            subscriptions,
            subscription =>
                subscription.Name == "Microsoft 365" &&
                subscription.Amount == 299m &&
                subscription.Currency == "PLN" &&
                subscription.BillingPeriod == "Yearly" &&
                subscription.MonthlyEquivalentAmount == 299m / 12m &&
                subscription.YearlyEquivalentAmount == 299m);
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
