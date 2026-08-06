using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
using SubscriptionManager.Blazor.Features.DigitalServices;
using SubscriptionManager.Blazor.Features.Subscriptions;

namespace SubscriptionManager.Blazor.Components.Pages;

public partial class Subscriptions
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "status")]
    public string? StatusQuery { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "category")]
    public string? CategoryQuery { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "customCategory")]
    public string? CustomCategoryQuery { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "subscriptionId")]
    public Guid[]? SubscriptionIdQuery { get; set; }

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask
    {
        get;
        set;
    } = default!;

    private System.Security.Claims.ClaimsPrincipal _user =
        new(new System.Security.Claims.ClaimsIdentity());

    private IReadOnlyList<SubscriptionResponse> _subscriptions = [];
    private IReadOnlyList<DigitalServiceResponse> _digitalServices = [];
    private SubscriptionCostSummaryResponse? _summary;

    private bool _loading = true;
    private bool _saving;
    private bool _categoryScrollInitialized;
    private bool _canScrollCategoriesLeft;
    private bool _canScrollCategoriesRight;

    private string? _pageError;
    private string? _dialogError;
    private string? _revealedCategory;
    private string _search = string.Empty;
    private string _catalogSearch = string.Empty;
    private string _category = AllCategories;

    private StatusFilter _status = StatusFilter.Active;
    private DialogKind _dialog;

    private SubscriptionResponse? _selected;
    private DigitalServiceResponse? _selectedService;

    private CreateDigitalServiceFormModel _customServiceForm = new();
    private SubscriptionFormModel _form = new();
    private EndSubscriptionModel _endModel = new();

    private ElementReference _categoryFiltersElement;
    private IJSObjectReference? _categoryScrollModule;
    private DotNetObjectReference<Subscriptions>? _dotNetReference;

    private const string AllCategories = "__all";

    private string CategoryNavigationClass =>
        $"{(_canScrollCategoriesLeft ? "can-scroll-left" : null)} " +
        $"{(_canScrollCategoriesRight ? "can-scroll-right" : null)}";

    private SubscriptionCostSummaryItemResponse? MostExpensive =>
        _summary?.TopSubscriptions.FirstOrDefault();

    private IReadOnlyDictionary<Guid, string> SubscriptionColors =>
        CreateSubscriptionColors();

    private IReadOnlyList<Guid> SelectedSubscriptionIds =>
        SubscriptionIdQuery?
            .Distinct()
            .ToArray() ??
        [];

    private IReadOnlyList<AppliedSubscriptionFilter>
        SelectedSubscriptionFilters =>
        SelectedSubscriptionIds
            .Select(subscriptionId =>
                new AppliedSubscriptionFilter(
                    subscriptionId,
                    _subscriptions
                        .FirstOrDefault(subscription =>
                            subscription.Id == subscriptionId)
                        ?.Name ??
                    T["Subscriptions.Filter.Unavailable"]))
            .ToArray();

    private IReadOnlyList<string> Categories =>
        new[] { AllCategories }
            .Concat(
                _subscriptions
                    .Select(CategoryKeyFor)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x))
            .ToArray();

    private IReadOnlyList<DigitalServiceResponse> FilteredServices =>
        _digitalServices
            .Where(x =>
                string.IsNullOrWhiteSpace(_catalogSearch) ||
                x.Name.Contains(
                    _catalogSearch,
                    StringComparison.CurrentCultureIgnoreCase) ||
                ServiceCategoryLabel(x).Contains(
                    _catalogSearch,
                    StringComparison.CurrentCultureIgnoreCase))
            .ToArray();

    private IEnumerable<SubscriptionResponse> Filtered =>
        _subscriptions
            .Where(subscription =>
                SelectedSubscriptionIds.Count == 0 ||
                SelectedSubscriptionIds.Contains(subscription.Id))
            .Where(MatchesStatus)
            .Where(x =>
                _category == AllCategories ||
                string.Equals(
                    CategoryKeyFor(x),
                    _category,
                    StringComparison.OrdinalIgnoreCase))
            .Where(x =>
                string.IsNullOrWhiteSpace(_search) ||
                x.Name.Contains(
                    _search,
                    StringComparison.CurrentCultureIgnoreCase) ||
                CategoryFor(x).Contains(
                    _search,
                    StringComparison.CurrentCultureIgnoreCase))
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Name);

    private IEnumerable<CategorySummary> CategorySummaries =>
        (_summary?.Categories ?? [])
            .Select(
                (summary, index) =>
                    new CategorySummary(
                        string.IsNullOrWhiteSpace(
                            summary.CustomCategoryName)
                            ? CategoryLabel(summary.Category)
                            : summary.CustomCategoryName!,
                        Tone(index),
                        summary.MonthlyCost));

    private string SummaryDescription
    {
        get
        {
            if (_summary?.ExchangeRateEffectiveDate is not { } date)
            {
                return T["Subscriptions.Description"];
            }

            return T[
                "Subscriptions.ExchangeRatesAsOf",
                FormatExchangeRateDate(date)];
        }
    }

    private string DialogTitle =>
        _dialog switch
        {
            DialogKind.Catalog =>
                T["Catalog.Title"],

            DialogKind.CustomService =>
                T["CustomService.Title"],

            DialogKind.Create =>
                T["Dialog.CreateTitle"],

            DialogKind.Edit =>
                T["Dialog.EditTitle"],

            DialogKind.End =>
                T["Dialog.EndTitle"],

            DialogKind.Delete =>
                T["Dialog.DeleteTitle"],

            _ => string.Empty
        };

    private string DialogDescription =>
        _dialog switch
        {
            DialogKind.Catalog =>
                T["Catalog.Description"],

            DialogKind.CustomService =>
                T["CustomService.Description"],

            DialogKind.Create =>
                T["Dialog.CreateDescription"],

            DialogKind.Edit =>
                T["Dialog.EditDescription"],

            DialogKind.End =>
                T["Dialog.EndDescription"],

            DialogKind.Delete =>
                T["Dialog.DeleteDescription"],

            _ => string.Empty
        };

    private string DialogIcon =>
        _dialog == DialogKind.Edit
            ? "edit"
            : _dialog == DialogKind.Delete
                ? "trash"
                : _dialog == DialogKind.CustomService
                    ? "plus"
                    : "subscriptions";

    protected override async Task OnInitializedAsync()
    {
        var authenticationState =
            await AuthenticationStateTask;

        _user = authenticationState.User;

        State.BaseCurrencyChanged +=
            OnBaseCurrencyChanged;

        await LoadAsync();
    }

    protected override void OnParametersSet()
    {
        if (!_loading)
        {
            ApplyQueryFilters();
        }
    }

    protected override async Task OnAfterRenderAsync(
        bool firstRender)
    {
        if (_loading ||
            _summary is null ||
            !string.IsNullOrWhiteSpace(_pageError))
        {
            return;
        }

        _categoryScrollModule ??=
            await JS.InvokeAsync<IJSObjectReference>(
                "import",
                "./Components/Pages/Subscriptions.razor.js");

        if (!_categoryScrollInitialized)
        {
            _dotNetReference ??=
                DotNetObjectReference.Create(this);

            await _categoryScrollModule.InvokeVoidAsync(
                "initialize",
                _categoryFiltersElement,
                _dotNetReference);

            _categoryScrollInitialized = true;
        }

        var revealSelected =
            !string.Equals(
                _revealedCategory,
                _category,
                StringComparison.OrdinalIgnoreCase);

        await _categoryScrollModule.InvokeVoidAsync(
            "refresh",
            _categoryFiltersElement,
            revealSelected);

        if (revealSelected)
        {
            _revealedCategory = _category;
        }
    }

    private void OnBaseCurrencyChanged()
    {
        _ = InvokeAsync(
            async () =>
            {
                await LoadAsync();
                StateHasChanged();
            });
    }

    private async Task LoadAsync()
    {
        await DisposeCategoryScrollAsync();

        _loading = true;
        _pageError = null;

        try
        {
            _digitalServices =
                await DigitalServicesApiClient.GetAllAsync(
                    _user);

            _subscriptions =
                await ApiClient.GetAllAsync(
                    _user);

            _summary =
                await ApiClient.GetCostSummaryAsync(
                    _user);
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode ==
                  System.Net.HttpStatusCode.Unauthorized)
        {
            SessionExpirationNavigation.RedirectToLogin(
                Navigation);
        }
        catch
        {
            _pageError = T["Error.Load"];
        }
        finally
        {
            _loading = false;
            ApplyQueryFilters();
        }
    }

    private void ApplyQueryFilters()
    {
        if (SelectedSubscriptionIds.Count > 0)
        {
            _status = StatusFilter.All;
            _category = AllCategories;
            return;
        }

        _status = ParseStatus(StatusQuery);

        var requestedCategory =
            !string.IsNullOrWhiteSpace(CategoryQuery)
                ? CategoryQuery
                : CustomCategoryQuery;

        _category =
            requestedCategory is not null &&
            Categories.Contains(
                requestedCategory,
                StringComparer.OrdinalIgnoreCase)
                ? Categories.First(category =>
                    string.Equals(
                        category,
                        requestedCategory,
                        StringComparison.OrdinalIgnoreCase))
                : AllCategories;
    }

    private static StatusFilter ParseStatus(
        string? value)
    {
        if (string.Equals(
                value,
                "all",
                StringComparison.OrdinalIgnoreCase))
        {
            return StatusFilter.All;
        }

        if (string.Equals(
                value,
                "ended",
                StringComparison.OrdinalIgnoreCase))
        {
            return StatusFilter.Ended;
        }

        return StatusFilter.Active;
    }

    private void SetStatus(
        StatusFilter status)
    {
        NavigateToFilters(
            status,
            _category);
    }

    private void SetCategory(
        string category)
    {
        NavigateToFilters(
            _status,
            category);
    }

    private void RemoveSubscriptionFilter(
        Guid subscriptionId)
    {
        var remainingIds =
            SelectedSubscriptionIds
                .Where(id => id != subscriptionId)
                .ToArray();

        if (remainingIds.Length == 0)
        {
            NavigateToFilters(
                StatusFilter.All,
                AllCategories);

            return;
        }

        var uri =
            Navigation.GetUriWithQueryParameters(
                "/subscriptions",
                new Dictionary<string, object?>
                {
                    ["subscriptionId"] = remainingIds
                });

        Navigation.NavigateTo(uri);
    }

    private async Task ScrollCategoriesAsync(
        int direction)
    {
        if (_categoryScrollModule is null ||
            !_categoryScrollInitialized)
        {
            return;
        }

        await _categoryScrollModule.InvokeVoidAsync(
            "scroll",
            _categoryFiltersElement,
            direction);
    }

    [JSInvokable]
    public Task UpdateCategoryScrollState(
        bool canScrollLeft,
        bool canScrollRight)
    {
        if (_canScrollCategoriesLeft == canScrollLeft &&
            _canScrollCategoriesRight == canScrollRight)
        {
            return Task.CompletedTask;
        }

        _canScrollCategoriesLeft = canScrollLeft;
        _canScrollCategoriesRight = canScrollRight;

        return InvokeAsync(StateHasChanged);
    }

    private void NavigateToFilters(
        StatusFilter status,
        string category)
    {
        string? predefinedCategory = null;
        string? customCategory = null;

        if (category != AllCategories)
        {
            if (IsCustomCategory(category))
            {
                customCategory = category;
            }
            else
            {
                predefinedCategory = category;
            }
        }

        var uri =
            Navigation.GetUriWithQueryParameters(
                "/subscriptions",
                new Dictionary<string, object?>
                {
                    ["status"] =
                        status == StatusFilter.Active
                            ? null
                            : status.ToString().ToLowerInvariant(),

                    ["category"] = predefinedCategory,
                    ["customCategory"] = customCategory,
                    ["subscriptionId"] = null
                });

        Navigation.NavigateTo(uri);
    }

    private bool IsCustomCategory(
        string category)
    {
        return _digitalServices.Any(service =>
            !service.IsPredefined &&
            string.Equals(
                service.CustomCategoryName,
                category,
                StringComparison.OrdinalIgnoreCase));
    }

    private void OpenCatalog()
    {
        _selected = null;
        _selectedService = null;
        _catalogSearch = string.Empty;
        _dialogError = null;
        _dialog = DialogKind.Catalog;
    }

    private void OpenCustomService()
    {
        _selectedService = null;
        _customServiceForm =
            new CreateDigitalServiceFormModel();

        _dialogError = null;
        _dialog = DialogKind.CustomService;
    }

    private void SelectService(
        DigitalServiceResponse service)
    {
        _selectedService = service;
    }

    private void ContinueToCreate()
    {
        if (_selectedService is null)
        {
            return;
        }

        _form = new SubscriptionFormModel
        {
            DigitalServiceId = _selectedService.Id,
            Name = _selectedService.Name
        };

        _dialog = DialogKind.Create;
    }

    private async Task CreateCustomServiceAsync()
    {
        _saving = true;
        _dialogError = null;

        try
        {
            var digitalServiceId =
                await DigitalServicesApiClient.CreateAsync(
                    _customServiceForm,
                    _user);

            _form = new SubscriptionFormModel
            {
                DigitalServiceId = digitalServiceId,
                Name = _customServiceForm.Name.Trim()
            };

            _dialog = DialogKind.Create;
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode ==
                  System.Net.HttpStatusCode.Unauthorized)
        {
            SessionExpirationNavigation.RedirectToLogin(
                Navigation);
        }
        catch
        {
            _dialogError = T["CustomService.Error"];
        }
        finally
        {
            _saving = false;
        }
    }

    private void OpenEdit(
        SubscriptionResponse subscription)
    {
        _selected = subscription;

        _form = new SubscriptionFormModel
        {
            DigitalServiceId =
                subscription.DigitalServiceId,

            Name = subscription.Name,
            Amount = subscription.Amount,
            Currency = subscription.Currency,

            BillingPeriod =
                subscription.BillingPeriod,

            StartDate = subscription.StartDate
        };

        _dialogError = null;
        _dialog = DialogKind.Edit;
    }

    private void OpenEnd(
        SubscriptionResponse subscription)
    {
        _selected = subscription;

        _endModel = new EndSubscriptionModel
        {
            EndDate =
                DateOnly.FromDateTime(DateTime.Today)
        };

        _dialogError = null;
        _dialog = DialogKind.End;
    }

    private void OpenDelete(
        SubscriptionResponse subscription)
    {
        _selected = subscription;
        _dialogError = null;
        _dialog = DialogKind.Delete;
    }

    private void CloseDialog()
    {
        if (_saving)
        {
            return;
        }

        _dialog = DialogKind.None;
        _selected = null;
        _selectedService = null;
        _dialogError = null;
    }

    private void ClearSearch()
    {
        _search = string.Empty;
    }

    private async Task SaveAsync()
    {
        _saving = true;
        _dialogError = null;

        try
        {
            if (_dialog == DialogKind.Create)
            {
                await ApiClient.CreateAsync(
                    _form,
                    _user);
            }
            else if (_selected is not null)
            {
                await ApiClient.UpdateAsync(
                    _selected.Id,
                    _form,
                    _user);
            }

            CloseDialogAfterSave();

            await LoadAsync();
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode ==
                  System.Net.HttpStatusCode.Unauthorized)
        {
            SessionExpirationNavigation.RedirectToLogin(
                Navigation);
        }
        catch
        {
            _dialogError = T["Error.Save"];
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task EndAsync()
    {
        if (_selected is null)
        {
            return;
        }

        _saving = true;
        _dialogError = null;

        try
        {
            await ApiClient.EndAsync(
                _selected.Id,
                _endModel.EndDate,
                _user);

            CloseDialogAfterSave();

            await LoadAsync();
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode ==
                  System.Net.HttpStatusCode.Unauthorized)
        {
            SessionExpirationNavigation.RedirectToLogin(
                Navigation);
        }
        catch
        {
            _dialogError = T["Error.Save"];
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task DeleteAsync()
    {
        if (_selected is null)
        {
            return;
        }

        _saving = true;
        _dialogError = null;

        try
        {
            await ApiClient.DeleteAsync(
                _selected.Id,
                _user);

            CloseDialogAfterSave();

            await LoadAsync();
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode ==
                  System.Net.HttpStatusCode.Unauthorized)
        {
            SessionExpirationNavigation.RedirectToLogin(
                Navigation);
        }
        catch
        {
            _dialogError = T["Error.Delete"];
        }
        finally
        {
            _saving = false;
        }
    }

    private void CloseDialogAfterSave()
    {
        _dialog = DialogKind.None;
        _selected = null;
        _selectedService = null;
        _dialogError = null;
    }

    private bool MatchesStatus(
        SubscriptionResponse subscription)
    {
        return _status == StatusFilter.All ||
               (_status == StatusFilter.Active
                   ? subscription.IsActive
                   : !subscription.IsActive);
    }

    private DigitalServiceResponse? ServiceFor(
        SubscriptionResponse subscription)
    {
        return subscription.DigitalServiceId is null
            ? null
            : _digitalServices.FirstOrDefault(service =>
                service.Id ==
                subscription.DigitalServiceId);
    }

    private string CategoryKeyFor(
        SubscriptionResponse subscription)
    {
        if (ServiceFor(subscription) is not { } service)
        {
            return "Other";
        }

        return !service.IsPredefined &&
               !string.IsNullOrWhiteSpace(
                   service.CustomCategoryName)
            ? service.CustomCategoryName
            : service.Category;
    }

    private string CategoryFor(
        SubscriptionResponse subscription)
    {
        return CategoryLabel(
            CategoryKeyFor(subscription));
    }

    private string? ManagementUrlFor(
        SubscriptionResponse subscription)
    {
        return ServiceFor(subscription)?.ManagementUrl;
    }

    private string ServiceCategoryLabel(
        DigitalServiceResponse service)
    {
        return !service.IsPredefined &&
               !string.IsNullOrWhiteSpace(
                   service.CustomCategoryName)
            ? service.CustomCategoryName
            : CategoryLabel(service.Category);
    }

    private string CategoryLabel(
        string category)
    {
        if (category == AllCategories)
        {
            return T["Filter.AllCategories"];
        }

        var key = $"Category.{category}";
        var value = T[key];

        return value == key
            ? category
            : value;
    }

    private IReadOnlyDictionary<Guid, string>
        CreateSubscriptionColors()
    {
        var colors =
            new Dictionary<Guid, string>();

        var position = 0;

        foreach (var subscription in
                 _summary?.ActiveSubscriptions ?? [])
        {
            colors[subscription.Id] =
                SubscriptionColorPalette.GetColor(
                    position);

            position++;
        }

        foreach (var subscription in
                 _subscriptions
                     .Where(subscription =>
                         !colors.ContainsKey(
                             subscription.Id))
                     .OrderByDescending(subscription =>
                         subscription.MonthlyEquivalentAmount)
                     .ThenBy(subscription =>
                         subscription.Name))
        {
            colors[subscription.Id] =
                SubscriptionColorPalette.GetColor(
                    position);

            position++;
        }

        return colors;
    }

    private string ColorFor(
        SubscriptionResponse subscription)
    {
        return SubscriptionColors.TryGetValue(
            subscription.Id,
            out var color)
            ? color
            : SubscriptionColorPalette.GetColor(0);
    }

    private string StyleFor(
        SubscriptionResponse subscription)
    {
        return $"--subscription-color: {ColorFor(subscription)}";
    }

    private string IconFor(
        SubscriptionResponse subscription)
    {
        return SubscriptionCategoryIconMapper.GetIcon(
            CategoryKeyFor(subscription));
    }

    private static string IconFor(
        DigitalServiceResponse service)
    {
        return SubscriptionCategoryIconMapper.GetIcon(
            service.Category);
    }

    private string Money(
        decimal value,
        Currency? currency = null)
    {
        var displayedCurrency =
            currency ??
            _summary?.BaseCurrency ??
            Currency.PLN;

        return string.Format(
            System.Globalization.CultureInfo.CurrentCulture,
            "{0:N2} {1}",
            value,
            displayedCurrency);
    }

    private static string Initials(
        string name)
    {
        return string.Concat(
            name.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part =>
                    char.ToUpperInvariant(part[0])));
    }

    private string Billing(
        BillingPeriod period)
    {
        return T[$"Billing.{period}"];
    }

    private string ShortBilling(
        BillingPeriod period)
    {
        return period switch
        {
            BillingPeriod.Monthly =>
                T["Billing.Short.Monthly"],

            BillingPeriod.Quarterly =>
                T["Billing.Short.Quarterly"],

            BillingPeriod.SemiAnnual =>
                T["Billing.Short.SemiAnnual"],

            BillingPeriod.Yearly =>
                T["Billing.Short.Yearly"],

            _ => string.Empty
        };
    }

    private static string FormatDate(
        DateOnly date)
    {
        return date.ToString(
            "d MMMM",
            System.Globalization.CultureInfo.CurrentCulture);
    }

    private static string FormatExchangeRateDate(
        DateOnly date)
    {
        return date.ToString(
            "d",
            System.Globalization.CultureInfo.CurrentCulture);
    }

    private static string Tone(
        int index)
    {
        return new[]
        {
            "orange",
            "violet",
            "green",
            "blue"
        }[index % 4];
    }

    private async ValueTask DisposeCategoryScrollAsync()
    {
        if (_categoryScrollModule is not null &&
            _categoryScrollInitialized)
        {
            try
            {
                await _categoryScrollModule.InvokeVoidAsync(
                    "dispose",
                    _categoryFiltersElement);
            }
            catch (JSDisconnectedException)
            {
            }
        }

        _categoryScrollInitialized = false;
        _canScrollCategoriesLeft = false;
        _canScrollCategoriesRight = false;
        _revealedCategory = null;
    }

    public async ValueTask DisposeAsync()
    {
        State.BaseCurrencyChanged -=
            OnBaseCurrencyChanged;

        await DisposeCategoryScrollAsync();

        _dotNetReference?.Dispose();

        if (_categoryScrollModule is not null)
        {
            try
            {
                await _categoryScrollModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }

    private enum StatusFilter
    {
        Active,
        All,
        Ended
    }

    private enum DialogKind
    {
        None,
        Catalog,
        CustomService,
        Create,
        Edit,
        End,
        Delete
    }

    private sealed record CategorySummary(
        string Name,
        string Tone,
        decimal Amount);

    private sealed record AppliedSubscriptionFilter(
        Guid Id,
        string Name);
}
