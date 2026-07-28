using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class UpdateSubscriptionTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly Guid NetflixId =
        Guid.Parse("7e25bbaa-130d-4f3a-8829-67592f433c01");

    private readonly HttpClient _client;

    public UpdateSubscriptionTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PutAsync_ShouldUpdateSubscription_WhenSubscriptionExists()
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

        var updateRequest = new
        {
            Name = "Spotify",
            Amount = 59m,
            Currency = "EUR",
            BillingPeriod = BillingPeriod.Yearly
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/subscriptions/{subscriptionId}",
            updateRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            updateResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/subscriptions/{subscriptionId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var subscription = await getResponse.Content
            .ReadFromJsonAsync<SubscriptionResponse>();

        Assert.NotNull(subscription);
        Assert.Equal(subscriptionId, subscription.Id);
        Assert.Equal("Spotify", subscription.Name);
        Assert.Equal(59m, subscription.Amount);
        Assert.Equal("EUR", subscription.Currency);
        Assert.Equal("Yearly", subscription.BillingPeriod);
        Assert.Equal(new DateOnly(2026, 1, 1), subscription.StartDate);
        Assert.Null(subscription.EndDate);
        Assert.True(subscription.IsActive);
    }


    [Fact]
    public async Task PutAsync_ShouldAssignDigitalService()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/subscriptions",
            new
            {
                Name = "Manual subscription",
                Amount = 49m,
                Currency = "PLN",
                BillingPeriod = BillingPeriod.Monthly,
                StartDate = new DateOnly(2026, 1, 1)
            });

        var subscriptionId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/subscriptions/{subscriptionId}",
            new
            {
                Name = "Personal Netflix",
                Amount = 59m,
                Currency = "PLN",
                BillingPeriod = BillingPeriod.Monthly,
                DigitalServiceId = NetflixId
            });

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var subscription = await _client
            .GetFromJsonAsync<SubscriptionResponse>(
                $"/api/subscriptions/{subscriptionId}");

        Assert.NotNull(subscription);
        Assert.Equal(NetflixId, subscription.DigitalServiceId);
        Assert.Equal("Personal Netflix", subscription.Name);
    }

    [Fact]
    public async Task PutAsync_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
    {
        var updateRequest = new
        {
            Name = "Spotify",
            Amount = 59m,
            Currency = "EUR",
            BillingPeriod = BillingPeriod.Yearly
        };

        var subscriptionId = Guid.NewGuid();
        var requestPath = $"/api/subscriptions/{subscriptionId}";

        var response = await _client.PutAsJsonAsync(
            requestPath,
            updateRequest);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);
    }

    private sealed record SubscriptionResponse(
        Guid Id,
        Guid? DigitalServiceId,
        string Name,
        decimal Amount,
        string Currency,
        string BillingPeriod,
        DateOnly StartDate,
        DateOnly? EndDate,
        bool IsActive);
}
