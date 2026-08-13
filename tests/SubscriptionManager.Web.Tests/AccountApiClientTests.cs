using System.Net;
using System.Security.Claims;
using SubscriptionManager.Web.Features.Account;
using SubscriptionManager.Web.Features.Authentication.Security;

namespace SubscriptionManager.Web.Tests;

public sealed class AccountApiClientTests
{
    [Fact]
    public async Task DeleteAccountAsync_SendsAuthorizedDeleteRequest()
    {
        HttpRequestMessage? capturedRequest = null;

        using var httpClient = new HttpClient(
            new StubHttpMessageHandler(request =>
            {
                capturedRequest = request;

                return new HttpResponseMessage(
                    HttpStatusCode.NoContent);
            }))
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var apiClient = new AccountApiClient(
            httpClient);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(
                    AuthenticationClaimTypes.AccessToken,
                    "access-token")
            ],
            "Test"));

        var deleted = await apiClient.DeleteAccountAsync(user);

        Assert.True(deleted);
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Delete, capturedRequest.Method);
        Assert.Equal(
            "https://api.example.com/api/account",
            capturedRequest.RequestUri?.ToString());
        Assert.Equal(
            "Bearer",
            capturedRequest.Headers.Authorization?.Scheme);
        Assert.Equal(
            "access-token",
            capturedRequest.Headers.Authorization?.Parameter);
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
