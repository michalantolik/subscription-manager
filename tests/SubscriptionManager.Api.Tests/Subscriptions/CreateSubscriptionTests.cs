using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class CreateSubscriptionTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly Guid NetflixId =
        Guid.Parse("7e25bbaa-130d-4f3a-8829-67592f433c01");

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateSubscriptionTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var request = new
        {
            Name = "Netflix",
            Amount = 49m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1)
        };

        var response = await client.PostAsJsonAsync(
            "/api/subscriptions",
            request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnBadRequest_WhenDigitalServiceBelongsToAnotherUser()
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

        var createDigitalServiceResponse =
            await firstUserClient.PostAsJsonAsync(
                "/api/digital-services",
                new
                {
                    Key = "private-service",
                    Name = "Private Service",
                    Category = DigitalServiceCategory.Other,
                    CustomCategoryName = "Private",
                    IconKey = "private-service",
                    ManagementUrl =
                        "https://example.com/account"
                });

        Assert.Equal(
            HttpStatusCode.Created,
            createDigitalServiceResponse.StatusCode);

        var digitalServiceId =
            await createDigitalServiceResponse.Content
                .ReadFromJsonAsync<Guid>();

        Assert.NotEqual(
            Guid.Empty,
            digitalServiceId);

        var request = new
        {
            Name = "Unavailable private service",
            Amount = 49m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            DigitalServiceId = digitalServiceId
        };

        var response =
            await secondUserClient.PostAsJsonAsync(
                "/api/subscriptions",
                request);

        await ProblemDetailsAssertions.AssertContainsAsync(
            response,
            HttpStatusCode.BadRequest,
            "Invalid request.",
            "The selected digital service is not available.",
            "/api/subscriptions");
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

        await ProblemDetailsAssertions.AssertContainsAsync(
            response,
            HttpStatusCode.BadRequest,
            "Invalid request.",
            "The selected digital service is not available.",
            "/api/subscriptions");
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
