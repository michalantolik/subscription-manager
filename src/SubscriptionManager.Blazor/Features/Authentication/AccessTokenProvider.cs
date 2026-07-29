using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace SubscriptionManager.Blazor.Features.Authentication;

public sealed class AccessTokenProvider(
    AuthenticationStateProvider authenticationStateProvider)
{
    public const string ClaimType =
        "subscription_manager:access_token";

    public async Task ApplyAsync(
        HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        var authenticationState =
            await authenticationStateProvider
                .GetAuthenticationStateAsync();

        var accessToken = authenticationState.User
            .FindFirst(ClaimType)?
            .Value;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = null;

            throw new InvalidOperationException(
                "The authenticated user access token is unavailable.");
        }

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }
}
