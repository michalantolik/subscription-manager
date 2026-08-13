using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using SubscriptionManager.Web.Common.FeatureToggles;
using SubscriptionManager.Web.Features.Billing;

namespace SubscriptionManager.Web.Features.Billing.Components;

public partial class BillingSettingsPanel
    : IAsyncDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } =
        default!;

    [Inject]
    private IFeatureToggleService FeatureToggleService { get; set; } =
        default!;

    [Parameter, EditorRequired]
    public ClaimsPrincipal User { get; set; } =
        default!;

    [Parameter]
    public EventCallback<BillingPlan> OnPlanChanged { get; set; }

    private CancellationTokenSource?
        _cancellationTokenSource;

    private BillingOverviewResponse? _overview;

    private IReadOnlyList<PaymentPlanPriceResponse>
        _prices = [];

    private SubscriptionChangePreviewResponse?
        _changePreview;

    private BillingSettingsStage _stage =
        BillingSettingsStage.Overview;

    private BillingInterval _selectedBillingInterval =
        BillingInterval.Monthly;

    private bool _isLoading = true;
    private bool _isLoadingPlans;
    private bool _isProcessing;

    private string? _error;
    private string? _plansError;
    private string? _operationError;

    private bool PaidPlansEnabled =>
        FeatureToggleService.IsEnabled(
            FeatureName.PaidPlans);

    private string PlanLabel =>
        GetPlanLabel(
            _overview?.Plan ??
            BillingPlan.Free);

    private string PlanDescription =>
        _overview?.Plan switch
        {
            BillingPlan.Plus =>
                T["Billing.Plan.PlusDescription"],

            BillingPlan.Premium =>
                T["Billing.Plan.PremiumDescription"],

            _ =>
                T["Billing.Plan.FreeDescription"]
        };

    private string BillingIntervalLabel =>
        GetBillingIntervalLabel(
            _overview?.BillingInterval);

    private string StatusLabel =>
        _overview?.Status switch
        {
            BillingSubscriptionStatus.Incomplete =>
                T["Billing.Status.Incomplete"],

            BillingSubscriptionStatus.Active =>
                T["Billing.Status.Active"],

            BillingSubscriptionStatus.PastDue =>
                T["Billing.Status.PastDue"],

            BillingSubscriptionStatus.Canceled =>
                T["Billing.Status.Canceled"],

            BillingSubscriptionStatus.Unpaid =>
                T["Billing.Status.Unpaid"],

            BillingSubscriptionStatus.Paused =>
                T["Billing.Status.Paused"],

            BillingSubscriptionStatus.IncompleteExpired =>
                T["Billing.Status.IncompleteExpired"],

            _ =>
                T["Billing.Settings.NotAvailable"]
        };

    private string StatusClass =>
        _overview?.Status switch
        {
            BillingSubscriptionStatus.Active =>
                "billing-status-active",

            BillingSubscriptionStatus.Incomplete or
            BillingSubscriptionStatus.PastDue =>
                "billing-status-warning",

            BillingSubscriptionStatus.Canceled or
            BillingSubscriptionStatus.Unpaid or
            BillingSubscriptionStatus.IncompleteExpired =>
                "billing-status-error",

            _ =>
                "billing-status-neutral"
        };

    private string RenewalDateLabel =>
        _overview?.CancelAtPeriodEnd == true
            ? T["Billing.Settings.AccessUntil"]
            : T["Billing.Settings.NextRenewal"];

    private string CurrentPeriodEndText =>
        FormatDate(
            _overview?.CurrentPeriodEnd);

    private string ChangeCurrentPlanLabel =>
        _changePreview is null
            ? string.Empty
            : GetPlanVariantLabel(
                _changePreview.CurrentPlan,
                _changePreview.CurrentBillingInterval);

    private string ChangeTargetPlanLabel =>
        _changePreview is null
            ? string.Empty
            : GetPlanVariantLabel(
                _changePreview.TargetPlan,
                _changePreview.TargetBillingInterval);

    private string ChangeEffectiveDateText =>
        FormatDate(
            _changePreview?.EffectiveAt);

    private string ChangeAmountDueNowText =>
        _changePreview is null
            ? string.Empty
            : FormatAmount(
                _changePreview.AmountDueNow,
                _changePreview.Currency);

    private string ChangeTimingLabel =>
        _changePreview?.Timing switch
        {
            BillingSubscriptionChangeTiming.Immediate =>
                T["Billing.Change.Timing.Immediate"],

            BillingSubscriptionChangeTiming.NextBillingPeriod =>
                T["Billing.Change.Timing.NextBillingPeriod"],

            _ =>
                T["Billing.Settings.NotAvailable"]
        };

    protected override async Task OnInitializedAsync()
    {
        _cancellationTokenSource =
            new CancellationTokenSource();

        await LoadOverviewAsync();
    }

    private async Task ReloadAsync()
    {
        await LoadOverviewAsync();
    }

    private async Task LoadOverviewAsync()
    {
        _isLoading = true;
        _error = null;
        _operationError = null;

        try
        {
            _overview =
                await BillingApiClient.GetOverviewAsync(
                    User,
                    GetCancellationToken());

            await OnPlanChanged.InvokeAsync(
                _overview.Plan);

            if (_overview.BillingInterval is { } billingInterval)
            {
                _selectedBillingInterval =
                    billingInterval;
            }

            _stage =
                BillingSettingsStage.Overview;
        }
        catch (HttpRequestException)
        {
            _error =
                T["Billing.Settings.LoadingError"];
        }
        catch (InvalidOperationException)
        {
            _error =
                T["Billing.Settings.LoadingError"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OpenPlanSelection()
    {
        if (_isProcessing)
        {
            return;
        }

        _operationError = null;
        _plansError = null;
        _changePreview = null;

        _stage =
            BillingSettingsStage.PlanSelection;

        if (_prices.Count == 0)
        {
            await LoadPlansAsync();
        }
    }

    private async Task LoadPlansAsync()
    {
        _isLoadingPlans = true;
        _plansError = null;

        try
        {
            _prices =
                await BillingApiClient.GetPlansAsync(
                    GetCancellationToken());

            if (_prices.Count == 0)
            {
                _plansError =
                    T["Billing.Plans.Empty"];
            }
        }
        catch (HttpRequestException)
        {
            _plansError =
                T["Billing.Plans.LoadingError"];
        }
        catch (InvalidOperationException)
        {
            _plansError =
                T["Billing.Plans.LoadingError"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isLoadingPlans = false;
        }
    }

    private void SelectBillingInterval(
        BillingInterval billingInterval)
    {
        if (_isProcessing)
        {
            return;
        }

        _selectedBillingInterval =
            billingInterval;

        _operationError = null;
    }

    private async Task SelectPlanAsync(
        BillingPlan plan)
    {
        if (_isProcessing ||
            _overview is null)
        {
            return;
        }

        _operationError = null;
        _changePreview = null;

        if (plan == BillingPlan.Free)
        {
            if (_overview.Plan == BillingPlan.Free)
            {
                return;
            }

            _stage =
                BillingSettingsStage.CancellationConfirmation;

            return;
        }

        if (!PaidPlansEnabled)
        {
            return;
        }

        if (_overview.Plan == plan &&
            _overview.BillingInterval ==
            _selectedBillingInterval)
        {
            return;
        }

        if (_overview.Plan == BillingPlan.Free)
        {
            await StartCheckoutAsync(
                plan);

            return;
        }

        await LoadChangePreviewAsync(
            plan);
    }

    private async Task StartCheckoutAsync(
        BillingPlan plan)
    {
        if (!PaidPlansEnabled)
        {
            return;
        }

        _isProcessing = true;
        _operationError = null;

        try
        {
            var successUrl =
                new Uri(
                    Navigation.GetUriWithQueryParameter(
                        "billing",
                        "success"));

            var cancelUrl =
                new Uri(
                    Navigation.GetUriWithQueryParameter(
                        "billing",
                        "cancel"));

            var checkoutUrl =
                await BillingApiClient
                    .CreateCheckoutSessionAsync(
                        plan,
                        _selectedBillingInterval,
                        successUrl,
                        cancelUrl,
                        User,
                        GetCancellationToken());

            Navigation.NavigateTo(
                checkoutUrl.ToString(),
                forceLoad: true);
        }
        catch (HttpRequestException)
        {
            _operationError =
                T["Billing.Checkout.Error"];
        }
        catch (InvalidOperationException)
        {
            _operationError =
                T["Billing.Checkout.Error"];
        }
        catch (UriFormatException)
        {
            _operationError =
                T["Billing.Checkout.Error"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task LoadChangePreviewAsync(
        BillingPlan plan)
    {
        if (!PaidPlansEnabled)
        {
            return;
        }

        _isProcessing = true;
        _operationError = null;

        try
        {
            _changePreview =
                await BillingApiClient.PreviewChangeAsync(
                    plan,
                    _selectedBillingInterval,
                    User,
                    GetCancellationToken());

            _stage =
                BillingSettingsStage.ChangeConfirmation;
        }
        catch (HttpRequestException)
        {
            _operationError =
                T["Billing.Change.PreviewError"];
        }
        catch (InvalidOperationException)
        {
            _operationError =
                T["Billing.Change.PreviewError"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task ConfirmChangeAsync()
    {
        if (_isProcessing ||
            _changePreview is null ||
            !PaidPlansEnabled)
        {
            return;
        }

        _isProcessing = true;
        _operationError = null;

        try
        {
            await BillingApiClient.ChangeAsync(
                _changePreview.TargetPlan,
                _changePreview.TargetBillingInterval,
                User,
                GetCancellationToken());

            _changePreview = null;

            await LoadOverviewAsync();
        }
        catch (HttpRequestException)
        {
            _operationError =
                T["Billing.Change.Error"];
        }
        catch (InvalidOperationException)
        {
            _operationError =
                T["Billing.Change.Error"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void OpenCancellationConfirmation()
    {
        if (_isProcessing ||
            _overview is null ||
            _overview.Plan == BillingPlan.Free)
        {
            return;
        }

        _operationError = null;
        _changePreview = null;

        _stage =
            BillingSettingsStage.CancellationConfirmation;
    }

    private async Task ConfirmCancellationAsync()
    {
        if (_isProcessing ||
            _overview is null ||
            _overview.Plan == BillingPlan.Free ||
            _overview.CancelAtPeriodEnd)
        {
            return;
        }

        _isProcessing = true;
        _operationError = null;

        try
        {
            await BillingApiClient.CancelAsync(
                User,
                GetCancellationToken());

            await LoadOverviewAsync();
        }
        catch (HttpRequestException)
        {
            _operationError =
                T["Billing.Cancellation.Error"];
        }
        catch (InvalidOperationException)
        {
            _operationError =
                T["Billing.Cancellation.Error"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task ResumeAsync()
    {
        if (_isProcessing ||
            _overview is null ||
            !_overview.CancelAtPeriodEnd)
        {
            return;
        }

        _isProcessing = true;
        _operationError = null;

        try
        {
            await BillingApiClient.ResumeAsync(
                User,
                GetCancellationToken());

            await LoadOverviewAsync();
        }
        catch (HttpRequestException)
        {
            _operationError =
                T["Billing.Settings.ResumeError"];
        }
        catch (InvalidOperationException)
        {
            _operationError =
                T["Billing.Settings.ResumeError"];
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void BackToOverview()
    {
        if (_isProcessing)
        {
            return;
        }

        _stage =
            BillingSettingsStage.Overview;

        _changePreview = null;
        _operationError = null;
        _plansError = null;
    }

    private PaymentPlanPriceResponse? GetPrice(
        BillingPlan plan)
    {
        return _prices.FirstOrDefault(
            price =>
                price.Plan == plan &&
                price.BillingInterval ==
                _selectedBillingInterval);
    }

    private string GetPriceText(
        BillingPlan plan)
    {
        var price =
            GetPrice(
                plan);

        return price is null
            ? T["Billing.Settings.NotAvailable"]
            : FormatAmount(
                price.Amount,
                price.Currency);
    }

    private string GetPlanVariantLabel(
        BillingPlan plan,
        BillingInterval billingInterval)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "{0} · {1}",
            GetPlanLabel(plan),
            GetBillingIntervalLabel(
                billingInterval));
    }

    private string GetPlanLabel(
        BillingPlan plan)
    {
        return plan switch
        {
            BillingPlan.Plus =>
                T["Billing.Plan.Plus"],

            BillingPlan.Premium =>
                T["Billing.Plan.Premium"],

            _ =>
                T["Billing.Plan.Free"]
        };
    }

    private string GetBillingIntervalLabel(
        BillingInterval? billingInterval)
    {
        return billingInterval switch
        {
            BillingInterval.Monthly =>
                T["Billing.Interval.Monthly"],

            BillingInterval.Yearly =>
                T["Billing.Interval.Yearly"],

            _ =>
                T["Billing.Settings.NotAvailable"]
        };
    }

    private static string FormatAmount(
        decimal amount,
        string currency)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "{0:N2} {1}",
            amount,
            currency.ToUpperInvariant());
    }

    private string FormatDate(
        DateTimeOffset? value)
    {
        return value is null
            ? T["Billing.Settings.NotAvailable"]
            : value.Value
                .ToLocalTime()
                .ToString(
                    "d",
                    CultureInfo.CurrentCulture);
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

    private enum BillingSettingsStage
    {
        Overview,
        PlanSelection,
        ChangeConfirmation,
        CancellationConfirmation
    }
}
