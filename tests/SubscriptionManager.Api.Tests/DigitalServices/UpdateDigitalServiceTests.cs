using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Api.Tests.DigitalServices;

public sealed class UpdateDigitalServiceTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UpdateDigitalServiceTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PutAsync_ShouldUpdateCustomDigitalService_WhenDigitalServiceExists()
    {
        var digitalServiceId = await CreateDigitalServiceAsync();

        var updateRequest = new
        {
            Key = "updated-service",
            Name = "Updated Service",
            Category = DigitalServiceCategory.Productivity,
            CustomCategoryName = (string?)null,
            IconKey = "updated",
            ManagementUrl = "https://example.com/settings"
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/digital-services/{digitalServiceId}",
            updateRequest);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var digitalService = await _client
            .GetFromJsonAsync<DigitalServiceResponse>(
                $"/api/digital-services/{digitalServiceId}");

        Assert.NotNull(digitalService);
        Assert.Equal(digitalServiceId, digitalService.Id);
        Assert.Equal("updated-service", digitalService.Key);
        Assert.Equal("Updated Service", digitalService.Name);
        Assert.Equal("Productivity", digitalService.Category);
        Assert.Null(digitalService.CustomCategoryName);
        Assert.Equal("updated", digitalService.IconKey);
        Assert.Equal(
            "https://example.com/settings",
            digitalService.ManagementUrl);
    }

    [Fact]
    public async Task PutAsync_ShouldReturnNotFound_WhenDigitalServiceIsPredefined()
    {
        var digitalServices = await _client
            .GetFromJsonAsync<DigitalServiceResponse[]>(
                "/api/digital-services");

        var netflix = Assert.Single(
            digitalServices!,
            digitalService => digitalService.Key == "netflix");

        var updateRequest = new
        {
            netflix.Key,
            Name = "Changed",
            Category = DigitalServiceCategory.Video,
            CustomCategoryName = (string?)null,
            netflix.IconKey,
            netflix.ManagementUrl
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/digital-services/{netflix.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutAsync_ShouldReturnNotFound_WhenDigitalServiceDoesNotExist()
    {
        var updateRequest = new
        {
            Key = "updated-service",
            Name = "Updated Service",
            Category = DigitalServiceCategory.Productivity,
            CustomCategoryName = (string?)null,
            IconKey = "updated",
            ManagementUrl = "https://example.com/settings"
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/digital-services/{Guid.NewGuid()}",
            updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> CreateDigitalServiceAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/digital-services",
            new
            {
                Key = "my-service",
                Name = "My Service",
                Category = DigitalServiceCategory.Other,
                CustomCategoryName = "Streaming",
                IconKey = "custom-service",
                ManagementUrl = "https://example.com/account"
            });

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private sealed record DigitalServiceResponse(
        Guid Id,
        string Key,
        string Name,
        string Category,
        string? CustomCategoryName,
        string? IconKey,
        string? ManagementUrl);
}
