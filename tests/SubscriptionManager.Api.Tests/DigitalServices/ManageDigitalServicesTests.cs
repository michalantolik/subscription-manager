using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Api.Tests.DigitalServices;

public sealed class ManageDigitalServicesTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ManageDigitalServicesTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAsync_ShouldCreateCustomDigitalService()
    {
        var response = await CreateDigitalServiceAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var id = await response.Content.ReadFromJsonAsync<Guid>();
        var digitalService = await _client.GetFromJsonAsync<DigitalServiceResponse>(
            $"/api/digital-services/{id}");

        Assert.NotNull(digitalService);
        Assert.Equal("my-service", digitalService.Key);
        Assert.Equal("My Service", digitalService.Name);
        Assert.False(digitalService.IsPredefined);
        Assert.Equal("Other", digitalService.Category);
        Assert.Equal("Streaming", digitalService.CustomCategoryName);
        Assert.True(digitalService.IsActive);
    }

    [Fact]
    public async Task PutAsync_ShouldUpdateCustomDigitalService()
    {
        var createResponse = await CreateDigitalServiceAsync();
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await _client.PutAsJsonAsync(
            $"/api/digital-services/{id}",
            new
            {
                Key = "updated-service",
                Name = "Updated Service",
                Category = DigitalServiceCategory.Productivity,
                CustomCategoryName = (string?)null,
                IconKey = "updated",
                ManagementUrl = "https://example.com/settings"
            });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var digitalService = await _client.GetFromJsonAsync<DigitalServiceResponse>(
            $"/api/digital-services/{id}");

        Assert.NotNull(digitalService);
        Assert.Equal("updated-service", digitalService.Key);
        Assert.Equal("Updated Service", digitalService.Name);
        Assert.Equal("Productivity", digitalService.Category);
    }

    [Fact]
    public async Task PutAsync_ShouldReturnNotFound_ForPredefinedDigitalService()
    {
        var services = await _client.GetFromJsonAsync<DigitalServiceResponse[]>(
            "/api/digital-services");
        var netflix = Assert.Single(services!, x => x.Key == "netflix");

        var requestPath =
            $"/api/digital-services/{netflix.Id}";

        var response = await _client.PutAsJsonAsync(
            requestPath,
            new
            {
                netflix.Key,
                Name = "Changed",
                Category = DigitalServiceCategory.Video,
                CustomCategoryName = (string?)null,
                netflix.IconKey,
                netflix.ManagementUrl
            });

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Digital service not found.",
            $"Digital service with id '{netflix.Id}' was not found.",
            requestPath);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldHideCustomDigitalService()
    {
        var createResponse = await CreateDigitalServiceAsync();
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await _client.PostAsync(
            $"/api/digital-services/{id}/deactivate",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var requestPath = $"/api/digital-services/{id}";
        var getResponse = await _client.GetAsync(requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            getResponse,
            HttpStatusCode.NotFound,
            "Digital service not found.",
            $"Digital service with id '{id}' was not found.",
            requestPath);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCustomDigitalService()
    {
        var createResponse = await CreateDigitalServiceAsync();
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await _client.DeleteAsync($"/api/digital-services/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var requestPath = $"/api/digital-services/{id}";
        var getResponse = await _client.GetAsync(requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            getResponse,
            HttpStatusCode.NotFound,
            "Digital service not found.",
            $"Digital service with id '{id}' was not found.",
            requestPath);
    }

    private Task<HttpResponseMessage> CreateDigitalServiceAsync()
    {
        return _client.PostAsJsonAsync(
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
