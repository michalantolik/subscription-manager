using System.Net;
using System.Net.Http.Json;
using SubscriptionManager.Web.Features.Authentication;
using SubscriptionManager.Web.Features.Localization;

namespace SubscriptionManager.Web.Tests;

public sealed class AuthenticationApiClientTests
{
    [Fact]
    public async Task LoginAsync_ReturnsSubscriptionPlan()
    {
        using var httpClient = new HttpClient(
            new StubHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new
                        {
                            AccessToken = "access-token",
                            Language = "English",
                            SubscriptionPlan = "Free"
                        })
                }))
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var apiClient = new AuthenticationApiClient(
            httpClient);

        var result = await apiClient.LoginAsync(
            "michal@example.com",
            "Test123!");

        Assert.True(result.Succeeded);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal(Language.English, result.Language);
        Assert.Equal("Free", result.SubscriptionPlan);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                responseFactory(request));
        }
    }
}
