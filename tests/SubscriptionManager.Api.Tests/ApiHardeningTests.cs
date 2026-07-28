using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Tests;

public sealed class ApiHardeningTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiHardeningTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnSecurityHeaders()
    {
        var response = await _client.GetAsync(
            "/api/subscriptions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AssertHeader(
            response,
            "X-Content-Type-Options",
            "nosniff");

        AssertHeader(
            response,
            "X-Frame-Options",
            "DENY");

        AssertHeader(
            response,
            "Referrer-Policy",
            "no-referrer");

        AssertHeader(
            response,
            "Permissions-Policy",
            "camera=(), microphone=(), geolocation=()");
    }

    [Fact]
    public async Task PostAsync_ShouldReturnBadRequestProblemDetails_WhenRequestIsInvalid()
    {
        var request = new
        {
            Name = "",
            Amount = 49m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1)
        };

        var response = await _client.PostAsJsonAsync(
            "/api/subscriptions",
            request);

        await ProblemDetailsAssertions.AssertContainsAsync(
            response,
            HttpStatusCode.BadRequest,
            "Invalid request.",
            "Subscription name is required.",
            "/api/subscriptions");
    }

    [Fact]
    public async Task PostAsync_ShouldReturnConflictProblemDetails_WhenOperationIsNotAllowed()
    {
        var digitalServiceId =
            await CreateDigitalServiceAsync();

        var firstResponse = await _client.PostAsync(
            $"/api/digital-services/{digitalServiceId}/deactivate",
            content: null);

        Assert.Equal(
            HttpStatusCode.NoContent,
            firstResponse.StatusCode);

        var secondResponse = await _client.PostAsync(
            $"/api/digital-services/{digitalServiceId}/deactivate",
            content: null);

        await ProblemDetailsAssertions.AssertContainsAsync(
            secondResponse,
            HttpStatusCode.Conflict,
            "The operation cannot be completed.",
            "The digital service is already inactive.",
            $"/api/digital-services/{digitalServiceId}/deactivate");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnConfiguredOpenApiDocument()
    {
        var response = await _client.GetAsync(
            "/openapi/v1.json");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var info = document.RootElement
            .GetProperty("info");

        Assert.Equal(
            "Subscription Manager API",
            info.GetProperty("title").GetString());

        Assert.Equal(
            "REST API for managing subscriptions.",
            info.GetProperty("description").GetString());

        Assert.Equal(
            "v1",
            info.GetProperty("version").GetString());
    }

    private async Task<Guid> CreateDigitalServiceAsync()
    {
        var uniqueKey =
            $"hardening-test-{Guid.NewGuid():N}";

        var response = await _client.PostAsJsonAsync(
            "/api/digital-services",
            new
            {
                Key = uniqueKey,
                Name = "Hardening Test Service",
                Category = DigitalServiceCategory.Other,
                CustomCategoryName = "Test",
                IconKey = (string?)null,
                ManagementUrl = (string?)null
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        return await response.Content
            .ReadFromJsonAsync<Guid>();
    }

    private static void AssertHeader(
        HttpResponseMessage response,
        string headerName,
        string expectedValue)
    {
        var headerExists = response.Headers.TryGetValues(
            headerName,
            out var values);

        Assert.True(
            headerExists,
            $"Response does not contain the {headerName} header.");

        Assert.Contains(
            expectedValue,
            values!);
    }
}
