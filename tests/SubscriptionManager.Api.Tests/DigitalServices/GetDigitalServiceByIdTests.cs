using System.Net;
using System.Net.Http.Json;

namespace SubscriptionManager.Api.Tests.DigitalServices;

public sealed class GetDigitalServiceByIdTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetDigitalServiceByIdTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSeededDigitalService_WhenDigitalServiceExists()
    {
        var digitalServices = await _client
            .GetFromJsonAsync<DigitalServiceResponse[]>(
                "/api/digital-services");

        var netflix = Assert.Single(
            digitalServices!,
            digitalService => digitalService.Key == "netflix");

        var response = await _client.GetAsync(
            $"/api/digital-services/{netflix.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var digitalService = await response.Content
            .ReadFromJsonAsync<DigitalServiceResponse>();

        Assert.NotNull(digitalService);
        Assert.Equal(netflix.Id, digitalService.Id);
        Assert.Equal("netflix", digitalService.Key);
        Assert.Equal("Netflix", digitalService.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenDigitalServiceDoesNotExist()
    {
        var digitalServiceId = Guid.NewGuid();

        var requestPath =
            $"/api/digital-services/{digitalServiceId}";

        var response = await _client.GetAsync(requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Digital service not found.",
            $"Digital service with id '{digitalServiceId}' was not found.",
            requestPath);
    }

    private sealed record DigitalServiceResponse(
        Guid Id,
        string Key,
        string Name);
}
