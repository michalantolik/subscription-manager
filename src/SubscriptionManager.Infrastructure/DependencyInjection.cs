using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Infrastructure.Persistence;
using SubscriptionManager.Infrastructure.Persistence.Repositories;

namespace SubscriptionManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "SubscriptionManager");

        services.AddDbContext<SubscriptionManagerDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        return services;
    }
}
