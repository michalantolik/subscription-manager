using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class CreateSubscriptionTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly Guid NetflixId =
        Guid.Parse("7e25bbaa-130d-4f3a-8829-67592f433c01");

    private readonly HttpClient _client;

    public CreateSubscriptionTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAsync_ShouldCreateManualSubscription()
    {
        var request = new
        {
            Name = "Netflix",
            Amount = 49m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1)
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/subscriptions",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        Assert.NotNull(createResponse.Headers.Location);

        var subscriptionId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, subscriptionId);

        var getResponse = await _client.GetAsync(
            createResponse.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var subscription =
            await getResponse.Content
                .ReadFromJsonAsync<SubscriptionResponse>();

        Assert.NotNull(subscription);
        Assert.Equal(subscriptionId, subscription.Id);
        Assert.Null(subscription.DigitalServiceId);
        Assert.Equal("Netflix", subscription.Name);
        Assert.Null(subscription.Category);
        Assert.Null(subscription.CustomCategoryName);
        Assert.Null(subscription.IconKey);
        Assert.Null(subscription.ManagementUrl);
        Assert.Equal(49m, subscription.Amount);
        Assert.Equal("PLN", subscription.Currency);
        Assert.Equal("Monthly", subscription.BillingPeriod);
        Assert.Equal(
            new DateOnly(2026, 1, 1),
            subscription.StartDate);
        Assert.Null(subscription.EndDate);
        Assert.True(subscription.IsActive);
    }

    [Fact]
    public async Task PostAsync_ShouldCreateSubscriptionFromAvailableDigitalService()
    {
        var request = new
        {
            Name = "Personal Netflix",
            Amount = 49m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            DigitalServiceId = NetflixId
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/subscriptions",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        Assert.NotNull(createResponse.Headers.Location);

        var subscriptionId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, subscriptionId);

        var getResponse = await _client.GetAsync(
            createResponse.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var subscription =
            await getResponse.Content
                .ReadFromJsonAsync<SubscriptionResponse>();

        Assert.NotNull(subscription);
        Assert.Equal(subscriptionId, subscription.Id);
        Assert.Equal(NetflixId, subscription.DigitalServiceId);
        Assert.Equal("Personal Netflix", subscription.Name);
        Assert.Equal("Video", subscription.Category);
        Assert.Null(subscription.CustomCategoryName);
        Assert.Equal("netflix", subscription.IconKey);
        Assert.Equal(
            "https://www.netflix.com/account",
            subscription.ManagementUrl);
        Assert.Equal(49m, subscription.Amount);
        Assert.Equal("PLN", subscription.Currency);
        Assert.Equal("Monthly", subscription.BillingPeriod);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnBadRequest_WhenDigitalServiceIsNotAvailable()
    {
        var request = new
        {
            Name = "Unavailable service",
            Amount = 49m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            DigitalServiceId =
                Guid.Parse(
                    "99999999-9999-9999-9999-999999999999")
        };

        var response = await _client.PostAsJsonAsync(
            "/api/subscriptions",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problemDetails =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal(
            "Invalid request.",
            problemDetails.Title);
        Assert.Contains(
            "The selected digital service is not available.",
            problemDetails.Detail);
        Assert.Equal(
            "/api/subscriptions",
            problemDetails.Instance);
    }

    private sealed record SubscriptionResponse(
        Guid Id,
        Guid? DigitalServiceId,
        string Name,
        string? Category,
        string? CustomCategoryName,
        string? IconKey,
        string? ManagementUrl,
        decimal Amount,
        string Currency,
        string BillingPeriod,
        DateOnly StartDate,
        DateOnly? EndDate,
        bool IsActive);
}
