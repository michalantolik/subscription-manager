using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Api.Tests.Account;

public sealed class AccountPreferencesEndpointTests
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

    public AccountPreferencesEndpointTests(
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
            "/api/account/preferences");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAccountPreferences_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();

        await SeedUserAsync(
            userId,
            Language.German,
            Currency.EUR);

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var response = await client.GetAsync(
            "/api/account/preferences");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var preferences =
            await response.Content
                .ReadFromJsonAsync<AccountPreferencesResponse>(
                    JsonOptions);

        Assert.NotNull(preferences);

        Assert.Equal(
            Language.German,
            preferences.Language);

        Assert.Equal(
            Currency.EUR,
            preferences.BaseCurrency);
    }

    [Fact]
    public async Task PutAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var request = new
        {
            Language = Language.English,
            BaseCurrency = Currency.USD
        };

        var response = await client.PutAsJsonAsync(
            "/api/account/preferences",
            request,
            JsonOptions);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task PutAsync_ShouldUpdateAccountPreferences_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();

        await SeedUserAsync(
            userId,
            Language.Polish,
            Currency.PLN);

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var request = new
        {
            Language = Language.English,
            BaseCurrency = Currency.USD
        };

        var updateResponse = await client.PutAsJsonAsync(
            "/api/account/preferences",
            request,
            JsonOptions);

        Assert.Equal(
            HttpStatusCode.NoContent,
            updateResponse.StatusCode);

        var getResponse = await client.GetAsync(
            "/api/account/preferences");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var preferences =
            await getResponse.Content
                .ReadFromJsonAsync<AccountPreferencesResponse>(
                    JsonOptions);

        Assert.NotNull(preferences);

        Assert.Equal(
            Language.English,
            preferences.Language);

        Assert.Equal(
            Currency.USD,
            preferences.BaseCurrency);
    }

    private async Task SeedUserAsync(
        Guid userId,
        Language language,
        Currency baseCurrency)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SubscriptionManagerDbContext>();

        var email =
            $"{userId}@example.com";

        dbContext.Users.Add(
            new ApplicationUser
            {
                Id = userId,
                UserName = email,
                NormalizedUserName =
                    email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail =
                    email.ToUpperInvariant(),
                EmailConfirmed = true,
                Language = language,
                BaseCurrency = baseCurrency
            });

        await dbContext.SaveChangesAsync();
    }

    private sealed record AccountPreferencesResponse(
        Language Language,
        Currency BaseCurrency);
}
