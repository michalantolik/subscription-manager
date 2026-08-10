using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class DeleteSubscriptionTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DeleteSubscriptionTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response =
            await client.DeleteAsync(
                $"/api/subscriptions/{Guid.NewGuid()}");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenSubscriptionBelongsToAnotherUser()
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

        var requestPath =
            $"/api/subscriptions/{subscriptionId}";

        var response =
            await secondUserClient.DeleteAsync(
                requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);

        var ownerGetResponse =
            await firstUserClient.GetAsync(
                requestPath);

        Assert.Equal(
            HttpStatusCode.OK,
            ownerGetResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteSubscription_WhenSubscriptionExists()
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

        var requestPath =
            $"/api/subscriptions/{subscriptionId}";

        var deleteResponse =
            await client.DeleteAsync(
                requestPath);

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var getResponse =
            await client.GetAsync(
                requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            getResponse,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
    {
        var userId =
            Guid.NewGuid();

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var subscriptionId =
            Guid.NewGuid();

        var requestPath =
            $"/api/subscriptions/{subscriptionId}";

        var response =
            await client.DeleteAsync(
                requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);
    }
}
