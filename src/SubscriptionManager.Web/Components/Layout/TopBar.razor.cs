using SubscriptionManager.Web.Common.Localization;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SubscriptionManager.Web.Features.Billing;
using SubscriptionManager.Web.Common.State;

namespace SubscriptionManager.Web.Components.Layout;

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

    private string? _subscriptionPlan;

    [Inject]
    private BillingApiClient BillingApiClient { get; set; } =
        default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider
    {
        get;
        set;
    } = default!;

    private string CurrentLanguage =>
        CultureInfo.CurrentUICulture
            .TwoLetterISOLanguageName;

    private string AccountMenuChevronClass =>
        _accountMenuOpen
            ? "account-menu-chevron open"
            : "account-menu-chevron";

    protected override async Task OnInitializedAsync()
    {
        var authenticationState =
            await AuthenticationStateProvider
                .GetAuthenticationStateAsync();

        var user =
            authenticationState.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return;
        }

        try
        {
            var billingOverview =
                await BillingApiClient.GetOverviewAsync(
                    user);

            _subscriptionPlan =
                billingOverview.Plan.ToString();
        }
        catch (HttpRequestException)
        {
            _subscriptionPlan = null;
        }
        catch (InvalidOperationException)
        {
            _subscriptionPlan = null;
        }
    }

    private void ToggleAccountMenu()
    {
        _accountMenuOpen =
            !_accountMenuOpen;
    }

    private void CloseAccountMenu()
    {
        _accountMenuOpen = false;
    }

    private void OpenAccountSettings()
    {
        _accountMenuOpen = false;
        _accountSettingsOpen = true;
    }

    private void CloseAccountSettings()
    {
        _accountSettingsOpen = false;
    }

    private void HandleBillingPlanChanged(
        BillingPlan plan)
    {
        _subscriptionPlan =
            plan.ToString();
    }

    private async Task ToggleThemeAsync()
    {
        State.ToggleTheme();

        await JS.InvokeVoidAsync(
            "subscriptionManagerTheme.set",
            (object)State.Theme,
            UiState.ThemeCookieName);
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
            string.IsNullOrWhiteSpace(
                currentPath)
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
    {
        return user.FindFirst(
                   ClaimTypes.Email)?.Value ??
               user.Identity?.Name ??
               "User";
    }

    private string? SubscriptionPlan(
        ClaimsPrincipal user)
    {
        _ = user;

        return _subscriptionPlan;
    }

    private static string Initial(
        ClaimsPrincipal user)
    {
        var displayName =
            DisplayName(
                user);

        return displayName.Length == 0
            ? "U"
            : char.ToUpperInvariant(
                    displayName[0])
                .ToString();
    }
}
