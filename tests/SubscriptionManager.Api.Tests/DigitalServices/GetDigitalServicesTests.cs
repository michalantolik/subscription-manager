using System.Net;
using System.Net.Http.Json;

namespace SubscriptionManager.Api.Tests.DigitalServices;

public sealed class GetDigitalServicesTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetDigitalServicesTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(
            "/api/digital-services");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnSeededDigitalServices()
    {
        var response = await _client.GetAsync(
            "/api/digital-services");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var digitalServices = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<DigitalServiceResponse>>();

        Assert.NotNull(digitalServices);
        Assert.Equal(198, digitalServices.Count);

        var netflix = Assert.Single(
            digitalServices,
            digitalService => digitalService.Key == "netflix");

        Assert.NotEqual(Guid.Empty, netflix.Id);
        Assert.Equal("Netflix", netflix.Name);
        Assert.Equal("Video", netflix.Category);
        Assert.Equal("netflix", netflix.IconKey);
        Assert.Equal(
            "https://www.netflix.com/account",
            netflix.ManagementUrl);

        Assert.Equal(
            digitalServices.Count,
            digitalServices
                .Select(digitalService => digitalService.Key)
                .Distinct()
                .Count());
    }

    private sealed record DigitalServiceResponse(
        Guid Id,
        string Key,
        string Name,
        string Category,
        string? IconKey,
        string? ManagementUrl);
}
