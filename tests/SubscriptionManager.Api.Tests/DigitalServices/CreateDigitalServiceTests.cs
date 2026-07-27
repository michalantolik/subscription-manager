using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Api.Tests.DigitalServices;

public sealed class CreateDigitalServiceTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateDigitalServiceTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAsync_ShouldCreateCustomDigitalService()
    {
        var createRequest = new
        {
            Key = "my-service",
            Name = "My Service",
            Category = DigitalServiceCategory.Other,
            CustomCategoryName = "Streaming",
            IconKey = "custom-service",
            ManagementUrl = "https://example.com/account"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/digital-services",
            createRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var digitalServiceId =
            await response.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, digitalServiceId);

        var digitalService = await _client
            .GetFromJsonAsync<DigitalServiceResponse>(
                $"/api/digital-services/{digitalServiceId}");

        Assert.NotNull(digitalService);
        Assert.Equal(digitalServiceId, digitalService.Id);
        Assert.Equal("my-service", digitalService.Key);
        Assert.Equal("My Service", digitalService.Name);
        Assert.False(digitalService.IsPredefined);
        Assert.Equal("Other", digitalService.Category);
        Assert.Equal("Streaming", digitalService.CustomCategoryName);
        Assert.Equal("custom-service", digitalService.IconKey);
        Assert.Equal(
            "https://example.com/account",
            digitalService.ManagementUrl);
        Assert.True(digitalService.IsActive);
    }

    private sealed record DigitalServiceResponse(
        Guid Id,
        string Key,
        string Name,
        bool IsPredefined,
        string Category,
        string? CustomCategoryName,
        string? IconKey,
        string? ManagementUrl,
        bool IsActive);
}
