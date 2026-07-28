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
    public async Task GetByIdAsync_ShouldReturnNotFoundProblemDetails_WhenSubscriptionDoesNotExist()
    {
        var subscriptionId = Guid.NewGuid();
        var requestPath = $"/api/subscriptions/{subscriptionId}";

        var response = await _client.GetAsync(requestPath);

        await ProblemDetailsAssertions.AssertAsync(
            response,
            HttpStatusCode.NotFound,
            "Subscription not found.",
            $"Subscription with id '{subscriptionId}' was not found.",
            requestPath);
    }
}
