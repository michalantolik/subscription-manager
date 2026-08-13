using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using SubscriptionManager.Blazor.Features.Account;
using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Billing;
using SubscriptionManager.Blazor.Features.Currencies;
using SubscriptionManager.Blazor.Features.Localization;

namespace SubscriptionManager.Blazor.Components.Layout;

public partial class AccountSettingsDialog
{
    [Parameter, EditorRequired]
    public ClaimsPrincipal User { get; set; } =
        default!;

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback<BillingPlan> OnBillingPlanChanged { get; set; }

    private CancellationTokenSource?
        _cancellationTokenSource;

    private DialogStage _stage =
        DialogStage.Settings;

    private AccountSettingsSection _activeSection =
        AccountSettingsSection.Preferences;

    private Language _language =
        Language.Polish;

    private Language _savedLanguage =
        Language.Polish;

    private Currency _baseCurrency =
        Currency.PLN;

    private Currency _savedBaseCurrency =
        Currency.PLN;

    private bool _isLoadingPreferences = true;
    private bool _isSavingPreferences;
    private bool _preferencesSaved;
    private bool _confirmed;

    private string? _preferencesError;
    private string? _error;

    private bool PreferencesChanged =>
        _language != _savedLanguage ||
        _baseCurrency != _savedBaseCurrency;

    private string Email =>
        User.FindFirst(ClaimTypes.Email)?.Value ??
        User.Identity?.Name ??
        T["Account.Settings.UnknownUser"];

    private string Initial =>
        Email.Length == 0
            ? "U"
            : char.ToUpperInvariant(
                Email[0])
                .ToString();

    private string DialogTitle =>
        _stage switch
        {
            DialogStage.Settings =>
                T["Account.Settings.Title"],

            DialogStage.Confirm =>
                T["Account.Delete.ConfirmTitle"],

            DialogStage.Deleting =>
                T["Account.Delete.DeletingTitle"],

            _ =>
                T["Account.Delete.SuccessTitle"]
        };

    private string DialogDescription =>
        _stage switch
        {
            DialogStage.Settings =>
                T["Account.Settings.Description"],

            DialogStage.Confirm =>
                T["Account.Delete.ConfirmDescription"],

            DialogStage.Deleting =>
                T["Account.Delete.DeletingDescription"],

            _ =>
                T["Account.Delete.SuccessDescription"]
        };

    protected override async Task OnInitializedAsync()
    {
        _cancellationTokenSource =
            new CancellationTokenSource();

        await LoadPreferencesAsync();
    }

    private async Task LoadPreferencesAsync()
    {
        _isLoadingPreferences = true;
        _preferencesSaved = false;
        _preferencesError = null;

        try
        {
            var preferences =
                await AccountApiClient.GetPreferencesAsync(
                    User,
                    GetCancellationToken());

            _language =
                preferences.Language;

            _savedLanguage =
                preferences.Language;

            _baseCurrency =
                preferences.BaseCurrency;

            _savedBaseCurrency =
                preferences.BaseCurrency;
        }
        catch (HttpRequestException)
        {
            _preferencesError =
                T["Account.Preferences.Error"];
        }
        catch (InvalidOperationException)
        {
            _preferencesError =
                T["Account.Preferences.Error"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isLoadingPreferences = false;
        }
    }

    private async Task SavePreferencesAsync()
    {
        if (_isSavingPreferences ||
            !PreferencesChanged)
        {
            return;
        }

        _isSavingPreferences = true;
        _preferencesSaved = false;
        _preferencesError = null;

        var languageChanged =
            _language != _savedLanguage;

        var baseCurrencyChanged =
            _baseCurrency != _savedBaseCurrency;

        try
        {
            await AccountApiClient.UpdatePreferencesAsync(
                _language,
                _baseCurrency,
                User,
                GetCancellationToken());

            _savedLanguage =
                _language;

            _savedBaseCurrency =
                _baseCurrency;

            if (baseCurrencyChanged)
            {
                State.NotifyBaseCurrencyChanged();
            }

            if (languageChanged)
            {
                ApplySelectedLanguage();

                return;
            }

            _preferencesSaved = true;
        }
        catch (HttpRequestException)
        {
            _preferencesError =
                T["Account.Preferences.Error"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isSavingPreferences = false;
        }
    }

    private void ApplySelectedLanguage()
    {
        var culture =
            _language.ToCultureName();

        var relativePath =
            Navigation.ToBaseRelativePath(
                Navigation.Uri);

        var redirectUri =
            string.IsNullOrWhiteSpace(
                relativePath)
                ? "/"
                : $"/{relativePath}";

        Navigation.NavigateTo(
            $"/culture/set?culture=" +
            $"{Uri.EscapeDataString(culture)}" +
            $"&redirectUri=" +
            $"{Uri.EscapeDataString(redirectUri)}",
            forceLoad: true);
    }

    private static string GetLanguageName(
        Language language)
    {
        return language switch
        {
            Language.Polish =>
                "Polski",

            Language.English =>
                "English",

            Language.German =>
                "Deutsch",

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(language),
                    language,
                    "The language is not supported.")
        };
    }

    private Task HandleBillingPlanChanged(
        BillingPlan plan)
    {
        return OnBillingPlanChanged.InvokeAsync(
            plan);
    }

    private void SelectSection(
        AccountSettingsSection section)
    {
        _activeSection =
            section;

        _preferencesSaved = false;
    }

    private string GetNavigationItemClass(
        AccountSettingsSection section)
    {
        return _activeSection == section
            ? "account-settings-navigation-item-active"
            : string.Empty;
    }

    private string? GetAriaCurrent(
        AccountSettingsSection section)
    {
        return _activeSection == section
            ? "page"
            : null;
    }

    private void OpenConfirmation()
    {
        _confirmed = false;
        _error = null;

        _stage =
            DialogStage.Confirm;
    }

    private void BackToSettings()
    {
        _confirmed = false;
        _error = null;

        _stage =
            DialogStage.Settings;

        _activeSection =
            AccountSettingsSection.Account;
    }

    private async Task DeleteAccountAsync()
    {
        if (!_confirmed ||
            _stage != DialogStage.Confirm)
        {
            return;
        }

        _error = null;

        _stage =
            DialogStage.Deleting;

        _cancellationTokenSource?.Dispose();

        _cancellationTokenSource =
            new CancellationTokenSource();

        try
        {
            var deleted =
                await AccountApiClient.DeleteAccountAsync(
                    User,
                    GetCancellationToken());

            if (!deleted)
            {
                _error =
                    T["Account.Delete.Error"];

                _stage =
                    DialogStage.Confirm;

                return;
            }

            _stage =
                DialogStage.Deleted;

            await Task.Delay(
                TimeSpan.FromSeconds(2),
                GetCancellationToken());

            Navigation.NavigateTo(
                "/authentication/account-deleted",
                forceLoad: true);
        }
        catch (HttpRequestException)
        {
            _error =
                T["Account.Delete.Error"];

            _stage =
                DialogStage.Confirm;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private Task CloseFromBackdrop()
    {
        return _stage is
            DialogStage.Settings or
            DialogStage.Confirm
                ? CloseAsync()
                : Task.CompletedTask;
    }

    private async Task CloseAsync()
    {
        if (_stage is
            DialogStage.Deleting or
            DialogStage.Deleted)
        {
            return;
        }

        await OnClose.InvokeAsync();
    }

    private CancellationToken GetCancellationToken()
    {
        return _cancellationTokenSource?.Token ??
               CancellationToken.None;
    }

    public ValueTask DisposeAsync()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();

        return ValueTask.CompletedTask;
    }

    private enum AccountSettingsSection
    {
        Preferences,
        Billing,
        Account
    }

    private enum DialogStage
    {
        Settings,
        Confirm,
        Deleting,
        Deleted
    }
}
