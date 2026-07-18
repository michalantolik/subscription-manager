using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.Subscriptions.CreateSubscription;

namespace SubscriptionManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateSubscriptionHandler>();

        return services;
    }
}
