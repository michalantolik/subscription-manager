using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class EndSubscriptionTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EndSubscriptionTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var endRequest = new
        {
            EndDate = new DateOnly(2026, 7, 21)
        };

        var response =
            await client.PostAsJsonAsync(
                $"/api/subscriptions/{Guid.NewGuid()}/end",
                endRequest);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnNotFound_WhenSubscriptionBelongsToAnotherUser()
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

        var subscriptionId =
            await createResponse.Content
                .ReadFromJsonAsync<Guid>();

        Assert.NotEqual(
            Guid.Empty,
            subscriptionId);

        var endRequest = new
        {
            EndDate = new DateOnly(2026, 7, 21)
        };

        var requestPath =
            $"/api/subscriptions/{subscriptionId}/end";

        var response =
            await secondUserClient.PostAsJsonAsync(
                requestPath,
                endRequest);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);

        var ownerGetResponse =
            await firstUserClient.GetAsync(
                $"/api/subscriptions/{subscriptionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            ownerGetResponse.StatusCode);

        var subscription =
            await ownerGetResponse.Content
                .ReadFromJsonAsync<SubscriptionResponse>();

        Assert.NotNull(subscription);
        Assert.Null(subscription.EndDate);
        Assert.True(subscription.IsActive);
    }

    [Fact]
    public async Task PostAsync_ShouldEndSubscription_WhenSubscriptionExists()
    {
        var userId =
            Guid.NewGuid();

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var createRequest = new
        {
            Name = "Netflix",
            Amount = 49m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1)
        };

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/subscriptions",
                createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var subscriptionId =
            await createResponse.Content
                .ReadFromJsonAsync<Guid>();

        Assert.NotEqual(
            Guid.Empty,
            subscriptionId);

        var endDate =
            new DateOnly(2026, 7, 21);

        var endRequest = new
        {
            EndDate = endDate
        };

        var endResponse =
            await client.PostAsJsonAsync(
                $"/api/subscriptions/{subscriptionId}/end",
                endRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            endResponse.StatusCode);

        var getResponse =
            await client.GetAsync(
                $"/api/subscriptions/{subscriptionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var subscription =
            await getResponse.Content
                .ReadFromJsonAsync<SubscriptionResponse>();

        Assert.NotNull(subscription);

        Assert.Equal(
            subscriptionId,
            subscription.Id);

        Assert.Equal(
            endDate,
            subscription.EndDate);

        Assert.False(
            subscription.IsActive);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
    {
        var userId =
            Guid.NewGuid();

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var endRequest = new
        {
            EndDate = new DateOnly(2026, 7, 21)
        };

        var subscriptionId =
            Guid.NewGuid();

        var requestPath =
            $"/api/subscriptions/{subscriptionId}/end";

        var response =
            await client.PostAsJsonAsync(
                requestPath,
                endRequest);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);
    }

    private sealed record SubscriptionResponse(
        Guid Id,
        string Name,
        decimal Amount,
        string Currency,
        string BillingPeriod,
        DateOnly StartDate,
        DateOnly? EndDate,
        bool IsActive);
}
