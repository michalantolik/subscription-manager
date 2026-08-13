using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SubscriptionManager.Web.Tests;

public sealed class SessionExpiredEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SessionExpiredEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SessionExpired_RedirectsToLogin()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            "/authentication/session-expired" +
            "?returnUrl=%2Fsubscriptions");

        Assert.Equal(
            HttpStatusCode.Redirect,
            response.StatusCode);

        Assert.Equal(
            "/login" +
            "?error=SessionExpired" +
            "&returnUrl=%2Fsubscriptions",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task SessionExpired_UsesRootWhenReturnUrlIsMissing()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            "/authentication/session-expired");

        Assert.Equal(
            HttpStatusCode.Redirect,
            response.StatusCode);

        Assert.Equal(
            "/login" +
            "?error=SessionExpired" +
            "&returnUrl=%2F",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("//example.com")]
    public async Task SessionExpired_RejectsUnsafeReturnUrl(
        string returnUrl)
    {
        using var client = CreateClient();

        var requestUri =
            "/authentication/session-expired" +
            $"?returnUrl={Uri.EscapeDataString(returnUrl)}";

        using var response = await client.GetAsync(requestUri);

        Assert.Equal(
            HttpStatusCode.Redirect,
            response.StatusCode);

        Assert.Equal(
            "/login" +
            "?error=SessionExpired" +
            "&returnUrl=%2F",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task SessionExpired_DeletesAuthenticationCookie()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            "/authentication/session-expired");

        var setCookieHeaders =
            response.Headers.TryGetValues(
                "Set-Cookie",
                out var values)
                ? values
                : [];

        Assert.Contains(
            setCookieHeaders,
            header =>
                header.Contains(
                    "__Host-SubscriptionManager.Authentication.v3=",
                    StringComparison.Ordinal) &&
                header.Contains(
                    "expires=",
                    StringComparison.OrdinalIgnoreCase));
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });
    }
}
