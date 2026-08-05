using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
using SubscriptionManager.Blazor.Features.SavingsPlans;
using SubscriptionManager.Blazor.Features.Subscriptions;
using System.Globalization;
using System.Security.Claims;

namespace SubscriptionManager.Blazor.Components.Pages;

public partial class SavingsPlan
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } =
        default!;

    private IReadOnlyList<SubscriptionResponse> Subscriptions = [];

    private readonly HashSet<Guid> _protectedSubscriptionIds = [];

    private ClaimsPrincipal? _user;
    private bool _isLoading = true;
    private bool _loadError;
    private decimal _currentMonthlyCost;
    private Currency _baseCurrency = Currency.PLN;
    private PlanStage _stage = PlanStage.Goal;
    private PlanStage _furthestStage = PlanStage.Goal;
    private SavingsPlanGoalType _goalKind = SavingsPlanGoalType.MonthlyBudget;
    private SavingsPlanStrategy _strategy = SavingsPlanStrategy.Balanced;
    private DialogKind _dialog;
    private decimal _targetAmount;
    private string _additionalPreference = string.Empty;
    private string? _validationMessage;
    private int _analysisStep;
    private SavingsPlanScenarioResponse? _recommendedPlan;
    private SavingsPlanScenarioResponse? _alternativePlan;

    private decimal CurrentMonthlyCost =>
        _currentMonthlyCost;

    private IReadOnlyList<(int Number, PlanStage Stage, string LabelKey)> FormSteps =>
    [
        (1, PlanStage.Goal, "SavingsPlan.Steps.Goal"),
        (2, PlanStage.Preferences, "SavingsPlan.Steps.Preferences"),
        (3, PlanStage.Review, "SavingsPlan.Steps.Review")
    ];

    private IReadOnlyList<(int Number, string LabelKey)> AnalysisSteps =>
    [
        (1, "SavingsPlan.Analysis.StepSubscriptions"),
        (2, "SavingsPlan.Analysis.StepPreferences"),
        (3, "SavingsPlan.Analysis.StepScenarios"),
        (4, "SavingsPlan.Analysis.StepRecommendations")
    ];

    private decimal RequiredSavings =>
        _goalKind == SavingsPlanGoalType.MonthlyBudget
            ? Math.Max(0m, CurrentMonthlyCost - _targetAmount)
            : Math.Max(0m, _targetAmount);

    private string GoalSummary =>
        _goalKind == SavingsPlanGoalType.MonthlyBudget
            ? T["SavingsPlan.Summary.BudgetValue", Money(_targetAmount)]
            : T["SavingsPlan.Summary.SavingsValue", Money(_targetAmount)];

    private string ProtectedSummary =>
        _protectedSubscriptionIds.Count == 0
            ? T["SavingsPlan.Summary.None"]
            : T["SavingsPlan.Summary.Selected", _protectedSubscriptionIds.Count];

    private string StrategyLabelKey => _strategy switch
    {
        SavingsPlanStrategy.FewerChanges => "SavingsPlan.Strategy.Fewer.Title",
        SavingsPlanStrategy.MaximumSavings => "SavingsPlan.Strategy.Maximum.Title",
        _ => "SavingsPlan.Strategy.Balanced.Title"
    };

    private IEnumerable<SubscriptionResponse> ProtectedSubscriptions =>
        Subscriptions.Where(subscription => _protectedSubscriptionIds.Contains(subscription.Id));

    private string ResultsDescription => _recommendedPlan switch
    {
        null => T["SavingsPlan.Results.NoPlanDescription"],
        { TargetReached: true } => T["SavingsPlan.Results.ReachedDescription", Money(_recommendedPlan.ProjectedMonthlyCost)],
        _ => T["SavingsPlan.Results.CloseDescription", Money(_recommendedPlan.ProjectedMonthlyCost)]
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        _loadError = false;

        try
        {
            var authenticationState =
                await AuthenticationStateTask;

            _user = authenticationState.User;

            var subscriptionsTask =
                SubscriptionsApiClient.GetAllAsync(
                    _user);

            var summaryTask =
                SubscriptionsApiClient.GetCostSummaryAsync(
                    _user);

            await Task.WhenAll(
                subscriptionsTask,
                summaryTask);

            Subscriptions =
                subscriptionsTask.Result
                    .Where(subscription => subscription.IsActive)
                    .OrderBy(subscription => subscription.Name)
                    .ToArray();

            var summary =
                summaryTask.Result;

            _currentMonthlyCost =
                summary.MonthlyCost;

            _baseCurrency =
                summary.BaseCurrency;

            SetDefaultTarget();
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
            _loadError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SelectGoal(SavingsPlanGoalType kind)
    {
        _goalKind = kind;
        SetDefaultTarget();
        _validationMessage = null;
    }

    private void SetDefaultTarget()
    {
        _targetAmount =
            _goalKind == SavingsPlanGoalType.MonthlyBudget
                ? Math.Round(
                    CurrentMonthlyCost * 0.75m,
                    2)
                : Math.Round(
                    CurrentMonthlyCost * 0.20m,
                    2);
    }

    private void ContinueFromGoal()
    {
        _validationMessage = null;

        if (_targetAmount <= 0)
        {
            _validationMessage = T["SavingsPlan.Validation.PositiveAmount"];
            return;
        }

        if (_goalKind == SavingsPlanGoalType.MonthlyBudget && _targetAmount >= CurrentMonthlyCost)
        {
            _validationMessage = T["SavingsPlan.Validation.BudgetTooHigh", Money(CurrentMonthlyCost)];
            return;
        }

        if (_goalKind == SavingsPlanGoalType.MonthlySavings && _targetAmount >= CurrentMonthlyCost)
        {
            _validationMessage = T["SavingsPlan.Validation.SavingsTooHigh", Money(CurrentMonthlyCost)];
            return;
        }

        MoveTo(PlanStage.Preferences);
    }

    private void ContinueFromPreferences()
    {
        _validationMessage = null;

        if (_protectedSubscriptionIds.Count == Subscriptions.Count)
        {
            _validationMessage = T["SavingsPlan.Validation.AllProtected"];
            return;
        }

        MoveTo(PlanStage.Review);
    }

    private async Task CreatePlanAsync()
    {
        if (_user is null)
        {
            return;
        }

        _validationMessage = null;
        _stage = PlanStage.Analyzing;
        _analysisStep = 3;

        try
        {
            var request =
                new CreateSavingsPlanRequest(
                    _goalKind,
                    _targetAmount,
                    _protectedSubscriptionIds.ToArray(),
                    _strategy,
                    string.IsNullOrWhiteSpace(
                        _additionalPreference)
                        ? null
                        : _additionalPreference.Trim(),
                    CultureInfo.CurrentUICulture
                        .TwoLetterISOLanguageName);

            var plan =
                await SavingsPlansApiClient.CreateAsync(
                    request,
                    _user);

            _analysisStep = 4;
            _baseCurrency = plan.BaseCurrency;
            _currentMonthlyCost = plan.CurrentMonthlyCost;
            _recommendedPlan = plan.Recommended;
            _alternativePlan = plan.Alternative;
            _stage = PlanStage.Results;
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode ==
                  System.Net.HttpStatusCode.Unauthorized)
        {
            SessionExpirationNavigation.RedirectToLogin(
                Navigation);
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode ==
                  System.Net.HttpStatusCode.ServiceUnavailable)
        {
            _stage = PlanStage.Review;
            _validationMessage =
                T["SavingsPlan.Error.Unavailable"];
        }
        catch
        {
            _stage = PlanStage.Review;
            _validationMessage =
                T["SavingsPlan.Error.Generate"];
        }
    }

    private void ToggleProtected(Guid id)
    {
        _validationMessage = null;
        if (!_protectedSubscriptionIds.Add(id))
        {
            _protectedSubscriptionIds.Remove(id);
        }
    }

    private void GoToStage(PlanStage stage)
    {
        if ((int)stage <= (int)_furthestStage)
        {
            _stage = stage;
            _validationMessage = null;
        }
    }

    private void MoveTo(PlanStage stage)
    {
        _stage = stage;
        if ((int)stage > (int)_furthestStage)
        {
            _furthestStage = stage;
        }
    }

    private void BackToGoal() => GoToStage(PlanStage.Goal);
    private void BackToPreferences() => GoToStage(PlanStage.Preferences);

    private void BackToPreferencesFromResults()
    {
        _stage = PlanStage.Preferences;
        _furthestStage = PlanStage.Review;
        _validationMessage = null;
    }

    private void StartOver()
    {
        _stage = PlanStage.Goal;
        _furthestStage = PlanStage.Goal;
        _goalKind = SavingsPlanGoalType.MonthlyBudget;
        _strategy = SavingsPlanStrategy.Balanced;
        SetDefaultTarget();
        _additionalPreference = string.Empty;
        _protectedSubscriptionIds.Clear();
        _recommendedPlan = null;
        _alternativePlan = null;
        _validationMessage = null;
    }

    private void ReviewChanges() => _dialog = DialogKind.ReviewChanges;
    private void CloseDialog() => _dialog = DialogKind.None;

    private void GoToSubscriptions()
    {
        CloseDialog();
        Navigation.NavigateTo("/subscriptions");
    }

    private string StepState(PlanStage stage) => stage switch
    {
        _ when stage == _stage => "active",
        _ when (int)stage < (int)_stage || (int)stage < (int)_furthestStage => "complete",
        _ => string.Empty
    };

    private static string Initials(string name) => string.Concat(
        name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));

    private string CategoryLabel(
        SubscriptionResponse subscription)
    {
        if (!string.IsNullOrWhiteSpace(
                subscription.CustomCategoryName))
        {
            return subscription.CustomCategoryName;
        }

        return CategoryLabel(
            subscription.Category);
    }

    private string CategoryLabel(
        string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return T["Category.Other"];
        }

        var localizationKey =
            $"Category.{category}";

        var localizedCategory =
            T[localizationKey];

        return localizedCategory == localizationKey
            ? category
            : localizedCategory;
    }

    private string Money(
        decimal amount,
        Currency? currency = null) =>
        $"{amount:N2} {currency ?? _baseCurrency}";

    private enum PlanStage
    {
        Goal = 1,
        Preferences = 2,
        Review = 3,
        Analyzing = 4,
        Results = 5
    }

    private enum DialogKind
    {
        None,
        ReviewChanges
    }
}
