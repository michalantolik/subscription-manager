using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace SubscriptionManager.Blazor.Features.Authentication;

public sealed class AccessTokenHandler(
    IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    private const string AccessTokenName = "access_token";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var httpContext =
            httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            throw new InvalidOperationException(
                "The current HTTP context is unavailable.");
        }

        var accessToken =
            await httpContext.GetTokenAsync(
                AccessTokenName);

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "The authenticated user access token is unavailable.");
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}
