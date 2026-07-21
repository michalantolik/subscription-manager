using System.Net;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class GetSubscriptionByIdTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetSubscriptionByIdTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
    {
        var subscriptionId = Guid.NewGuid();

        var response = await _client.GetAsync(
            $"/api/subscriptions/{subscriptionId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}
