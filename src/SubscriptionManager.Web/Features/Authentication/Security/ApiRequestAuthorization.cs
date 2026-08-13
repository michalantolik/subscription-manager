using System.Net.Http.Headers;
using System.Security.Claims;

namespace SubscriptionManager.Web.Features.Authentication.Security;

/// <summary>
/// Provides authorization for authenticated API requests.
/// </summary>
/// <remarks>
/// The API access token is obtained from the current Blazor circuit user.
/// The principal is supplied by the component to avoid resolving authentication
/// state from an unrelated HttpClientFactory scope.
/// </remarks>
public static class ApiRequestAuthorization
{
    public static void AddBearerToken(
        HttpRequestMessage request,
        ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(user);

        var accessToken = user
            .FindFirst(AuthenticationClaimTypes.AccessToken)?
            .Value;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "The authenticated session does not contain an API access token.");
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }
}
