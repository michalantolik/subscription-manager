using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class DeleteSubscriptionTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DeleteSubscriptionTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteSubscription_WhenSubscriptionExists()
    {
        var createRequest = new
        {
            Name = "Netflix",
            Amount = 49m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1)
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/subscriptions",
            createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var subscriptionId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, subscriptionId);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/subscriptions/{subscriptionId}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var requestPath = $"/api/subscriptions/{subscriptionId}";
        var getResponse = await _client.GetAsync(requestPath);

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
        var subscriptionId = Guid.NewGuid();

        var requestPath = $"/api/subscriptions/{subscriptionId}";

        var response = await _client.DeleteAsync(requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);
    }
}
