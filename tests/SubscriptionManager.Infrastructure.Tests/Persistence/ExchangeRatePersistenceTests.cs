using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Domain.ExchangeRates;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Tests.Persistence;

public sealed class ExchangeRatePersistenceTests
{
    [Fact]
    public async Task Currency_ShouldBeStoredAsString_AndMappedBackToEnum()
    {
        await using var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<
                    SubscriptionManagerDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var dbContext =
            new SubscriptionManagerDbContext(
                options);

        await dbContext.Database.EnsureCreatedAsync();

        var exchangeRate =
            new ExchangeRate(
                Currency.CHF,
                4.5m,
                new DateOnly(2026, 8, 1),
                new DateTimeOffset(
                    2026,
                    8,
                    2,
                    12,
                    0,
                    0,
                    TimeSpan.Zero));

        dbContext.ExchangeRates.Add(
            exchangeRate);

        await dbContext.SaveChangesAsync();

        await using (var command =
                     connection.CreateCommand())
        {
            command.CommandText =
                """
                SELECT Currency
                FROM ExchangeRates
                WHERE Currency = 'CHF'
                """;

            var storedCurrency =
                await command.ExecuteScalarAsync();

            Assert.Equal(
                "CHF",
                storedCurrency);
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO ExchangeRates
                (Currency, RateToPln, EffectiveDate, LastCheckedAt)
            VALUES
                ('EUR', 4.3, '2026-08-01', '2026-08-02 12:00:00+00:00')
            """);

        dbContext.ChangeTracker.Clear();

        var loadedExchangeRate =
            await dbContext.ExchangeRates
                .AsNoTracking()
                .SingleAsync(rate =>
                    rate.Currency == Currency.EUR);

        Assert.Equal(
            Currency.EUR,
            loadedExchangeRate.Currency);

        Assert.Equal(
            4.3m,
            loadedExchangeRate.RateToPln);

        Assert.Equal(
            new DateOnly(2026, 8, 1),
            loadedExchangeRate.EffectiveDate);
    }
}
