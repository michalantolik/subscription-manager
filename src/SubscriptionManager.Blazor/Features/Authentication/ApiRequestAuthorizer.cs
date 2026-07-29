using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace SubscriptionManager.Blazor.Features.Authentication;

public sealed class ApiRequestAuthorizer(
    AuthenticationStateProvider authenticationStateProvider)
{
    public async Task AuthorizeAsync(
        HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authenticationState =
            await authenticationStateProvider
                .GetAuthenticationStateAsync();

        var accessToken = authenticationState.User
            .FindFirst(AuthenticationClaimTypes.AccessToken)?
            .Value;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "The authenticated session does not contain an API access token. Sign out and sign in again.");
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }
}
