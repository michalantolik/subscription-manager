using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SubscriptionManager.Api.Tests.Authentication;

public sealed class GetCurrentUserEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public GetCurrentUserEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUnauthorized_WhenAccessTokenIsInvalid()
    {
        using var client =
            _factory.CreateJwtClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "invalid-access-token");

        var response = await client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCurrentUser_WhenUserIsAuthenticated()
    {
        using var client =
            _factory.CreateClient();

        var response = await client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var currentUser = await response.Content
            .ReadFromJsonAsync<CurrentUserResponse>();

        Assert.NotNull(currentUser);

        Assert.NotEqual(
            Guid.Empty,
            currentUser.UserId);
    }

    private sealed record CurrentUserResponse(
        Guid UserId);
}
