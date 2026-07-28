using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Api.Tests.DigitalServices;

public sealed class DeleteDigitalServiceTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DeleteDigitalServiceTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCustomDigitalService_WhenDigitalServiceExists()
    {
        var digitalServiceId = await CreateDigitalServiceAsync();

        var response = await _client.DeleteAsync(
            $"/api/digital-services/{digitalServiceId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var requestPath =
            $"/api/digital-services/{digitalServiceId}";
        var getResponse = await _client.GetAsync(requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            getResponse,
            HttpStatusCode.NotFound,
            "Digital service not found.",
            $"Digital service with id '{digitalServiceId}' was not found.",
            requestPath);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenDigitalServiceDoesNotExist()
    {
        var digitalServiceId = Guid.NewGuid();
        var requestPath =
            $"/api/digital-services/{digitalServiceId}";

        var response = await _client.DeleteAsync(requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Digital service not found.",
            $"Digital service with id '{digitalServiceId}' was not found.",
            requestPath);
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
