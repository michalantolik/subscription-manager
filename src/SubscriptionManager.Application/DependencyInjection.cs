using Microsoft.Extensions.DependencyInjection;
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
        services.AddScoped<CreateSubscriptionHandler>();
        services.AddScoped<GetSubscriptionsHandler>();
        services.AddScoped<GetSubscriptionByIdHandler>();
        services.AddScoped<UpdateSubscriptionHandler>();
        services.AddScoped<EndSubscriptionHandler>();
        services.AddScoped<DeleteSubscriptionHandler>();

        return services;
    }
}
