using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class GetSubscriptionByIdTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetSubscriptionByIdTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(
            $"/api/subscriptions/{Guid.NewGuid()}");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenSubscriptionBelongsToAnotherUser()
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

        var subscriptionId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(
            Guid.Empty,
            subscriptionId);

        var requestPath =
            $"/api/subscriptions/{subscriptionId}";

        var response =
            await secondUserClient.GetAsync(requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFoundProblemDetails_WhenSubscriptionDoesNotExist()
    {
        var subscriptionId = Guid.NewGuid();
        var requestPath =
            $"/api/subscriptions/{subscriptionId}";

        var response = await _client.GetAsync(
            requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);
    }
}
