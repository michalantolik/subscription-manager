using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Api.Tests.DigitalServices;

public sealed class DeactivateDigitalServiceTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DeactivateDigitalServiceTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAsync_ShouldDeactivateCustomDigitalService_WhenDigitalServiceExists()
    {
        var digitalServiceId = await CreateDigitalServiceAsync();

        var response = await _client.PostAsync(
            $"/api/digital-services/{digitalServiceId}/deactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/digital-services/{digitalServiceId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnNotFound_WhenDigitalServiceDoesNotExist()
    {
        var response = await _client.PostAsync(
            $"/api/digital-services/{Guid.NewGuid()}/deactivate",
            content: null);

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
}
