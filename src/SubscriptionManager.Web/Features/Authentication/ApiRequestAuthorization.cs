using System.Net.Http.Headers;
using System.Security.Claims;

namespace SubscriptionManager.Web.Features.Authentication;

/// <summary>
/// Adds the API access token from the current Blazor circuit user.
/// The principal is supplied by the component so authentication state
/// isn't resolved from an unrelated HttpClientFactory scope.
/// </summary>
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
