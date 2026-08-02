using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.DigitalServices.CreateDigitalService;
using SubscriptionManager.Application.DigitalServices.DeactivateDigitalService;
using SubscriptionManager.Application.DigitalServices.DeleteDigitalService;
using SubscriptionManager.Application.DigitalServices.GetDigitalServiceById;
using SubscriptionManager.Application.DigitalServices.GetDigitalServices;
using SubscriptionManager.Application.DigitalServices.UpdateDigitalService;
using SubscriptionManager.Application.Identity.ConfirmEmail;
using SubscriptionManager.Application.Identity.DeleteUser;
using SubscriptionManager.Application.Identity.ForgotPassword;
using SubscriptionManager.Application.Identity.GetBaseCurrency;
using SubscriptionManager.Application.Identity.LoginUser;
using SubscriptionManager.Application.Identity.RegisterUser;
using SubscriptionManager.Application.Identity.ResetPassword;
using SubscriptionManager.Application.Identity.UpdateBaseCurrency;
using SubscriptionManager.Application.Subscriptions.CreateSubscription;
using SubscriptionManager.Application.Subscriptions.DeleteSubscription;
using SubscriptionManager.Application.Subscriptions.EndSubscription;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionById;
using SubscriptionManager.Application.Subscriptions.GetSubscriptions;
using SubscriptionManager.Application.Subscriptions.UpdateSubscription;

namespace SubscriptionManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateDigitalServiceHandler>();
        services.AddScoped<GetDigitalServicesHandler>();
        services.AddScoped<GetDigitalServiceByIdHandler>();
        services.AddScoped<UpdateDigitalServiceHandler>();
        services.AddScoped<DeactivateDigitalServiceHandler>();
        services.AddScoped<DeleteDigitalServiceHandler>();

        services.AddScoped<CreateSubscriptionHandler>();
        services.AddScoped<GetSubscriptionsHandler>();
        services.AddScoped<GetSubscriptionByIdHandler>();
        services.AddScoped<UpdateSubscriptionHandler>();
        services.AddScoped<EndSubscriptionHandler>();
        services.AddScoped<DeleteSubscriptionHandler>();

        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<ConfirmEmailHandler>();
        services.AddScoped<LoginUserHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<GetBaseCurrencyHandler>();
        services.AddScoped<UpdateBaseCurrencyHandler>();
        services.AddScoped<DeleteUserHandler>();

        return services;
    }
}
