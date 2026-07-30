using Microsoft.AspNetCore.Components;

namespace SubscriptionManager.Blazor.Features.Authentication;

public static class SessionExpirationNavigation
{
    public static void RedirectToLogin(
        NavigationManager navigation)
    {
        var returnUrl = navigation.ToBaseRelativePath(navigation.Uri);

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = "/";
        }
        else
        {
            returnUrl = "/" + returnUrl;
        }

        var encodedReturnUrl = Uri.EscapeDataString(returnUrl);

        navigation.NavigateTo(
            $"/authentication/session-expired?returnUrl={encodedReturnUrl}",
            forceLoad: true);
    }
}
