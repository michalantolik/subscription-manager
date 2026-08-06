using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;

public sealed class CreateSavingsPlanHandler
{
    private const int MaximumAdditionalPreferenceLength = 300;

    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IIdentityService _identityService;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ICurrentUser _currentUser;
    private readonly ISavingsPlanAgent _savingsPlanAgent;
    private readonly ISavingsPlanUsageRepository
        _savingsPlanUsageRepository;

    public CreateSavingsPlanHandler(
        ISubscriptionRepository subscriptionRepository,
        IIdentityService identityService,
        IExchangeRateService exchangeRateService,
        ICurrentUser currentUser,
        ISavingsPlanAgent savingsPlanAgent,
        ISavingsPlanUsageRepository savingsPlanUsageRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _identityService = identityService;
        _exchangeRateService = exchangeRateService;
        _currentUser = currentUser;
        _savingsPlanAgent = savingsPlanAgent;
        _savingsPlanUsageRepository =
            savingsPlanUsageRepository;
    }

    public async Task<SavingsPlanDto> HandleAsync(
        CreateSavingsPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidateCommand(command);

        var userId = _currentUser.UserId;
        var usageDateUtc =
            DateOnly.FromDateTime(DateTime.UtcNow);

        var subscriptions =
            await _subscriptionRepository.GetAllAsync(
                userId,
                cancellationToken);

        var baseCurrency =
            await _identityService.GetBaseCurrencyAsync(
                userId,
                cancellationToken);

        if (baseCurrency is null)
        {
            throw new InvalidOperationException(
                "The current user's base currency is unavailable.");
        }

        var subscriptionPlan =
            await _identityService.GetSubscriptionPlanAsync(
                userId,
                cancellationToken);

        if (subscriptionPlan is null)
        {
            throw new InvalidOperationException(
                "The current user's subscription plan is unavailable.");
        }

        var dailyRequestLimit =
            SubscriptionPlanLimits
                .GetDailySavingsPlanLimit(
                    subscriptionPlan.Value);

        var activeSubscriptions =
            subscriptions
                .Where(subscription =>
                    subscription.IsActive)
                .ToArray();

        if (activeSubscriptions.Length == 0)
        {
            var remainingRequests =
                await _savingsPlanUsageRepository
                    .GetRemainingRequestCountAsync(
                        userId,
                        usageDateUtc,
                        dailyRequestLimit,
                        cancellationToken);

            return new SavingsPlanDto(
                baseCurrency.Value,
                0m,
                null,
                null,
                subscriptionPlan.Value,
                dailyRequestLimit,
                remainingRequests);
        }

        CurrentExchangeRates? exchangeRates = null;

        if (activeSubscriptions.Any(subscription =>
                subscription.Currency != baseCurrency.Value))
        {
            exchangeRates =
                await _exchangeRateService.GetCurrentAsync(
                    cancellationToken);
        }

        var availableSubscriptions =
            activeSubscriptions
                .Select(subscription =>
                    CreateSubscription(
                        subscription,
                        baseCurrency.Value,
                        exchangeRates))
                .ToArray();

        var currentMonthlyCost =
            availableSubscriptions.Sum(
                subscription =>
                    subscription.MonthlyCost);

        ValidateTarget(
            command,
            currentMonthlyCost);

        var protectedSubscriptionIds =
            command.ProtectedSubscriptionIds
                .Distinct()
                .ToHashSet();

        ValidateProtectedSubscriptions(
            protectedSubscriptionIds,
            availableSubscriptions);

        var agentRequest =
            new SavingsPlanAgentRequest(
                command.GoalType,
                command.TargetAmount,
                command.Strategy,
                NormalizeAdditionalPreference(
                    command.AdditionalPreference),
                NormalizeLanguageCode(
                    command.LanguageCode),
                baseCurrency.Value,
                currentMonthlyCost,
                protectedSubscriptionIds,
                availableSubscriptions);

        var remainingRequestCount =
            await _savingsPlanUsageRepository
                .TryRegisterRequestAsync(
                    userId,
                    usageDateUtc,
                    dailyRequestLimit,
                    cancellationToken);

        if (remainingRequestCount is null)
        {
            throw new SavingsPlanUsageLimitExceededException(
                dailyRequestLimit);
        }

        var agentResult =
            await _savingsPlanAgent.CreatePlanAsync(
                agentRequest,
                cancellationToken);

        var subscriptionsById =
            availableSubscriptions.ToDictionary(
                subscription => subscription.Id);

        var recommended =
            CreateScenario(
                agentResult.Recommended,
                command,
                currentMonthlyCost,
                protectedSubscriptionIds,
                subscriptionsById);

        var alternative =
            CreateScenario(
                agentResult.Alternative,
                command,
                currentMonthlyCost,
                protectedSubscriptionIds,
                subscriptionsById);

        if (recommended is not null &&
            alternative is not null &&
            recommended.Subscriptions
                .Select(subscription =>
                    subscription.Id)
                .Order()
                .SequenceEqual(
                    alternative.Subscriptions
                        .Select(subscription =>
                            subscription.Id)
                        .Order()))
        {
            alternative = null;
        }

        return new SavingsPlanDto(
            baseCurrency.Value,
            currentMonthlyCost,
            recommended,
            alternative,
            subscriptionPlan.Value,
            dailyRequestLimit,
            remainingRequestCount.Value);
    }

    private static void ValidateCommand(
        CreateSavingsPlanCommand command)
    {
        if (!Enum.IsDefined(command.GoalType))
        {
            throw new ArgumentException(
                "Savings plan goal type is invalid.",
                nameof(command.GoalType));
        }

        if (!Enum.IsDefined(command.Strategy))
        {
            throw new ArgumentException(
                "Savings plan strategy is invalid.",
                nameof(command.Strategy));
        }

        if (command.TargetAmount <= 0m)
        {
            throw new ArgumentException(
                "Target amount must be greater than zero.",
                nameof(command.TargetAmount));
        }

        ArgumentNullException.ThrowIfNull(
            command.ProtectedSubscriptionIds);

        if (command.ProtectedSubscriptionIds.Any(
                id => id == Guid.Empty))
        {
            throw new ArgumentException(
                "Protected subscription identifiers cannot be empty.",
                nameof(command.ProtectedSubscriptionIds));
        }

        if (command.AdditionalPreference?.Length >
            MaximumAdditionalPreferenceLength)
        {
            throw new ArgumentException(
                $"Additional preference cannot exceed {MaximumAdditionalPreferenceLength} characters.",
                nameof(command.AdditionalPreference));
        }

        _ = NormalizeLanguageCode(
            command.LanguageCode);
    }

    private static void ValidateTarget(
        CreateSavingsPlanCommand command,
        decimal currentMonthlyCost)
    {
        if (command.TargetAmount >= currentMonthlyCost)
        {
            var message =
                command.GoalType ==
                SavingsPlanGoalType.MonthlyBudget
                    ? "Monthly budget must be lower than the current monthly cost."
                    : "Monthly savings must be lower than the current monthly cost.";

            throw new ArgumentException(
                message,
                nameof(command.TargetAmount));
        }
    }

    private static void ValidateProtectedSubscriptions(
        IReadOnlySet<Guid> protectedSubscriptionIds,
        IReadOnlyCollection<SavingsPlanSubscriptionDto>
            availableSubscriptions)
    {
        var availableIds =
            availableSubscriptions
                .Select(subscription =>
                    subscription.Id)
                .ToHashSet();

        if (protectedSubscriptionIds.Any(
                id => !availableIds.Contains(id)))
        {
            throw new ArgumentException(
                "A protected subscription is not active or does not belong to the current user.",
                nameof(protectedSubscriptionIds));
        }

        if (protectedSubscriptionIds.Count ==
            availableSubscriptions.Count)
        {
            throw new ArgumentException(
                "At least one subscription must remain available for the savings plan.",
                nameof(protectedSubscriptionIds));
        }
    }

    private static SavingsPlanSubscriptionDto CreateSubscription(
        Subscription subscription,
        Currency baseCurrency,
        CurrentExchangeRates? exchangeRates)
    {
        var monthlyCost =
            subscription.Currency == baseCurrency
                ? subscription.MonthlyEquivalentAmount
                : exchangeRates?.Convert(
                    subscription.MonthlyEquivalentAmount,
                    subscription.Currency,
                    baseCurrency)
                  ?? throw new InvalidOperationException(
                      "Current exchange rates are unavailable.");

        var category =
            !string.IsNullOrWhiteSpace(
                subscription.CustomCategoryName)
                ? subscription.CustomCategoryName.Trim()
                : (subscription.Category ??
                   DigitalServiceCategory.Other).ToString();

        return new SavingsPlanSubscriptionDto(
            subscription.Id,
            subscription.Name,
            category,
            monthlyCost);
    }

    private static SavingsPlanScenarioDto? CreateScenario(
        SavingsPlanAgentScenario? agentScenario,
        CreateSavingsPlanCommand command,
        decimal currentMonthlyCost,
        IReadOnlySet<Guid> protectedSubscriptionIds,
        IReadOnlyDictionary<Guid, SavingsPlanSubscriptionDto>
            subscriptionsById)
    {
        if (agentScenario is null)
        {
            return null;
        }

        var subscriptionIds =
            agentScenario.SubscriptionIds?
                .Distinct()
                .ToArray()
            ?? throw InvalidAgentResult();

        if (subscriptionIds.Length == 0 ||
            subscriptionIds.Any(
                id =>
                    !subscriptionsById.ContainsKey(id) ||
                    protectedSubscriptionIds.Contains(id)))
        {
            throw InvalidAgentResult();
        }

        if (string.IsNullOrWhiteSpace(
                agentScenario.Explanation))
        {
            throw InvalidAgentResult();
        }

        var selectedSubscriptions =
            subscriptionIds
                .Select(id => subscriptionsById[id])
                .ToArray();

        var monthlySavings =
            selectedSubscriptions.Sum(
                subscription =>
                    subscription.MonthlyCost);

        var projectedMonthlyCost =
            Math.Max(
                0m,
                currentMonthlyCost - monthlySavings);

        var targetReached =
            command.GoalType switch
            {
                SavingsPlanGoalType.MonthlyBudget =>
                    projectedMonthlyCost <=
                    command.TargetAmount,

                SavingsPlanGoalType.MonthlySavings =>
                    monthlySavings >=
                    command.TargetAmount,

                _ => false
            };

        return new SavingsPlanScenarioDto(
            selectedSubscriptions,
            projectedMonthlyCost,
            monthlySavings,
            monthlySavings * 12m,
            targetReached,
            agentScenario.Explanation.Trim());
    }

    private static string? NormalizeAdditionalPreference(
        string? additionalPreference)
    {
        return string.IsNullOrWhiteSpace(
            additionalPreference)
            ? null
            : additionalPreference.Trim();
    }

    private static string NormalizeLanguageCode(
        string languageCode)
    {
        return languageCode?
            .Trim()
            .ToLowerInvariant() switch
        {
            "pl" => "pl",
            "en" => "en",
            "de" => "de",

            _ => throw new ArgumentException(
                "Language code must be 'pl', 'en' or 'de'.",
                nameof(languageCode))
        };
    }

    private static InvalidOperationException InvalidAgentResult()
    {
        return new InvalidOperationException(
            "The savings plan agent returned an invalid result.");
    }
}
