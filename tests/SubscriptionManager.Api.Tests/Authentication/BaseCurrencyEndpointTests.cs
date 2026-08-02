using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Api.Tests.Authentication;

public sealed class BaseCurrencyEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    private readonly CustomWebApplicationFactory _factory;

    public BaseCurrencyEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(
            "/api/auth/account/base-currency");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnBaseCurrency_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();

        await CreateUserAsync(
            userId,
            Currency.EUR);

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var response = await client.GetAsync(
            "/api/auth/account/base-currency");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<BaseCurrencyResponse>(
                JsonOptions);

        Assert.NotNull(result);

        Assert.Equal(
            Currency.EUR,
            result.BaseCurrency);
    }

    [Fact]
    public async Task PutAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response = await client.PutAsJsonAsync(
            "/api/auth/account/base-currency",
            new UpdateBaseCurrencyRequest(
                Currency.EUR),
            JsonOptions);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task PutAsync_ShouldUpdateBaseCurrency_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();

        await CreateUserAsync(
            userId,
            Currency.PLN);

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var response = await client.PutAsJsonAsync(
            "/api/auth/account/base-currency",
            new UpdateBaseCurrencyRequest(
                Currency.EUR),
            JsonOptions);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var baseCurrency =
            await dbContext.Users
                .AsNoTracking()
                .Where(user =>
                    user.Id == userId)
                .Select(user =>
                    user.BaseCurrency)
                .SingleAsync();

        Assert.Equal(
            Currency.EUR,
            baseCurrency);
    }

    private async Task CreateUserAsync(
        Guid userId,
        Currency baseCurrency)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        dbContext.Users.Add(
            new ApplicationUser
            {
                Id = userId,
                UserName = $"{userId}@example.com",
                Email = $"{userId}@example.com",
                BaseCurrency = baseCurrency
            });

        await dbContext.SaveChangesAsync();
    }

    private sealed record BaseCurrencyResponse(
        Currency BaseCurrency);

    private sealed record UpdateBaseCurrencyRequest(
        Currency BaseCurrency);
}
