using System.Net;

namespace SubscriptionManager.Api.Tests.Account;

public sealed class DeleteAccountEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DeleteAccountEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response = await client.DeleteAsync(
            "/api/account");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
