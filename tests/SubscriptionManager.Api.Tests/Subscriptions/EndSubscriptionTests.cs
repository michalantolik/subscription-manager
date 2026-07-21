using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class EndSubscriptionTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EndSubscriptionTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAsync_ShouldEndSubscription_WhenSubscriptionExists()
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

        var endDate = new DateOnly(2026, 7, 21);

        var endRequest = new
        {
            EndDate = endDate
        };

        var endResponse = await _client.PostAsJsonAsync(
            $"/api/subscriptions/{subscriptionId}/end",
            endRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            endResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/subscriptions/{subscriptionId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var subscription = await getResponse.Content
            .ReadFromJsonAsync<SubscriptionResponse>();

        Assert.NotNull(subscription);
        Assert.Equal(subscriptionId, subscription.Id);
        Assert.Equal(endDate, subscription.EndDate);
        Assert.False(subscription.IsActive);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
    {
        var endRequest = new
        {
            EndDate = new DateOnly(2026, 7, 21)
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/subscriptions/{Guid.NewGuid()}/end",
            endRequest);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
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
