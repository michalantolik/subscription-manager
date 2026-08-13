using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.Account.DeleteAccount;
using SubscriptionManager.Application.Account.GetAccountPreferences;
using SubscriptionManager.Application.Account.UpdateAccountPreferences;
using SubscriptionManager.Application.Authentication.ConfirmEmail;
using SubscriptionManager.Application.Authentication.ForgotPassword;
using SubscriptionManager.Application.Authentication.LoginUser;
using SubscriptionManager.Application.Authentication.RegisterUser;
using SubscriptionManager.Application.Authentication.ResetPassword;
using SubscriptionManager.Application.Billing.CancelSubscription;
using SubscriptionManager.Application.Billing.ChangeSubscription;
using SubscriptionManager.Application.Billing.CreateCheckoutSession;
using SubscriptionManager.Application.Billing.GetBillingOverview;
using SubscriptionManager.Application.Billing.GetPaymentPlans;
using SubscriptionManager.Application.Billing.PreviewSubscriptionChange;
using SubscriptionManager.Application.Billing.ProcessWebhook;
using SubscriptionManager.Application.Billing.ResumeSubscription;
using SubscriptionManager.Application.DigitalServices.CreateDigitalService;
using SubscriptionManager.Application.DigitalServices.DeactivateDigitalService;
using SubscriptionManager.Application.DigitalServices.DeleteDigitalService;
using SubscriptionManager.Application.DigitalServices.GetDigitalServiceById;
using SubscriptionManager.Application.DigitalServices.GetDigitalServices;
using SubscriptionManager.Application.DigitalServices.UpdateDigitalService;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;
using SubscriptionManager.Application.SavingsPlans.GetSavingsPlanUsage;
using SubscriptionManager.Application.Subscriptions.CreateSubscription;
using SubscriptionManager.Application.Subscriptions.DeleteSubscription;
using SubscriptionManager.Application.Subscriptions.EndSubscription;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionById;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionCostSummary;
using SubscriptionManager.Application.Subscriptions.GetSubscriptions;
using SubscriptionManager.Application.Subscriptions.UpdateSubscription;

namespace SubscriptionManager.Application;

/// <summary>
/// Registers application services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton(
            TimeProvider.System);

        services.AddScoped<CreateCheckoutSessionHandler>();
        services.AddScoped<GetBillingOverviewHandler>();
        services.AddScoped<GetPaymentPlansHandler>();
        services.AddScoped<PreviewSubscriptionChangeHandler>();
        services.AddScoped<ChangeSubscriptionHandler>();
        services.AddScoped<CancelSubscriptionHandler>();
        services.AddScoped<ResumeSubscriptionHandler>();
        services.AddScoped<ProcessPaymentWebhookHandler>();

        services.AddScoped<CreateDigitalServiceHandler>();
        services.AddScoped<GetDigitalServicesHandler>();
        services.AddScoped<GetDigitalServiceByIdHandler>();
        services.AddScoped<UpdateDigitalServiceHandler>();
        services.AddScoped<DeactivateDigitalServiceHandler>();
        services.AddScoped<DeleteDigitalServiceHandler>();

        services.AddScoped<
            IExchangeRateService,
            ExchangeRateService>();

        services.AddScoped<CreateSubscriptionHandler>();
        services.AddScoped<GetSubscriptionsHandler>();
        services.AddScoped<GetSubscriptionByIdHandler>();
        services.AddScoped<GetSubscriptionCostSummaryHandler>();
        services.AddScoped<UpdateSubscriptionHandler>();
        services.AddScoped<EndSubscriptionHandler>();
        services.AddScoped<DeleteSubscriptionHandler>();

        services.AddScoped<CreateSavingsPlanHandler>();
        services.AddScoped<GetSavingsPlanUsageHandler>();

        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<ConfirmEmailHandler>();
        services.AddScoped<LoginUserHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<GetAccountPreferencesHandler>();
        services.AddScoped<UpdateAccountPreferencesHandler>();
        services.AddScoped<DeleteAccountHandler>();

        return services;
    }
}
