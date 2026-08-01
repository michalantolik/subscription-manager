using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SubscriptionManager.Blazor.Tests;

public sealed class AccountDeletedEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AccountDeletedEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AccountDeleted_RedirectsToLoginWithConfirmation()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            "/authentication/account-deleted");

        Assert.Equal(
            HttpStatusCode.Redirect,
            response.StatusCode);

        Assert.Equal(
            "/login?status=account-deleted",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AccountDeleted_DeletesAuthenticationCookie()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            "/authentication/account-deleted");

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
