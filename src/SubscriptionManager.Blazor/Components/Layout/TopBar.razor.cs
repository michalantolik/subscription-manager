using Microsoft.JSInterop;
using SubscriptionManager.Blazor.Services;
using System.Globalization;
using System.Security.Claims;

namespace SubscriptionManager.Blazor.Components.Layout;

public partial class TopBar
{
    private static readonly (string Code, string Label)[] Languages =
    [
        ("pl", "PL"),
        ("en", "EN"),
        ("de", "DE")
    ];

    private bool _accountMenuOpen;
    private bool _accountSettingsOpen;

    private string CurrentLanguage =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    private string AccountMenuChevronClass =>
        _accountMenuOpen
            ? "account-menu-chevron open"
            : "account-menu-chevron";

    private void ToggleAccountMenu()
        => _accountMenuOpen = !_accountMenuOpen;

    private void CloseAccountMenu()
        => _accountMenuOpen = false;

    private void OpenAccountSettings()
    {
        _accountMenuOpen = false;
        _accountSettingsOpen = true;
    }

    private void CloseAccountSettings()
        => _accountSettingsOpen = false;

    private async Task ToggleThemeAsync()
    {
        State.ToggleTheme();

        await JS.InvokeVoidAsync(
            "subscriptionManagerTheme.set",
            State.Theme,
            AppState.ThemeCookieName);
    }

    private void ChangeLanguage(
        string languageCode)
    {
        var culture =
            SupportedCultures.ByLanguageCode.TryGetValue(
                languageCode,
                out var selected)
                ? selected
                : SupportedCultures.DefaultCulture;

        var currentPath =
            Navigation.ToBaseRelativePath(
                Navigation.Uri);

        var redirectUri =
            string.IsNullOrWhiteSpace(currentPath)
                ? "/"
                : $"/{currentPath}";

        Navigation.NavigateTo(
            "/culture/set" +
            $"?culture={Uri.EscapeDataString(culture)}" +
            $"&redirectUri={Uri.EscapeDataString(redirectUri)}",
            forceLoad: true);
    }

    private static string DisplayName(
        ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Email)?.Value ??
           user.Identity?.Name ??
           "User";

    private static string Initial(
        ClaimsPrincipal user)
    {
        var displayName = DisplayName(user);

        return displayName.Length == 0
            ? "U"
            : char.ToUpperInvariant(
                displayName[0]).ToString();
    }
}
