using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Infrastructure.DigitalServices;

namespace SubscriptionManager.Infrastructure.Persistence;

/// <summary>
/// Provides database migration and seed operations.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Migrates the database and seeds required data in an idempotent manner.
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await serviceProvider.MigrateDatabaseAsync(cancellationToken);
        await serviceProvider.SeedDatabaseAsync(cancellationToken);
    }

    /// <summary>
    /// Applies pending database migrations.
    /// </summary>
    public static async Task MigrateDatabaseAsync(
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
    }

    /// <summary>
    /// Seeds required application data in an idempotent manner.
    /// </summary>
    public static async Task SeedDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        await DigitalServiceSeed.SeedAsync(
            dbContext,
            cancellationToken);
    }
}
