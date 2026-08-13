using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Infrastructure.DigitalServices;

namespace SubscriptionManager.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes the application database and seed data in an idempotent manner.
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await DigitalServiceSeed.SeedAsync(
            dbContext,
            cancellationToken);
    }
}
