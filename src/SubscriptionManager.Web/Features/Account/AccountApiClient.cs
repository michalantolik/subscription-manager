using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using SubscriptionManager.Web.Common.Currencies;
using SubscriptionManager.Web.Common.Localization;
using SubscriptionManager.Web.Features.Authentication.Security;

namespace SubscriptionManager.Web.Features.Account;

/// <summary>
/// Provides access to account-related API operations.
/// </summary>
public sealed class AccountApiClient(
    HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    public async Task<AccountPreferences> GetPreferencesAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/account/preferences");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<AccountPreferences>(
                       JsonOptions,
                       cancellationToken)
               ?? throw new InvalidOperationException(
                   "The account preferences response was empty.");
    }

    public async Task UpdatePreferencesAsync(
        Language language,
        Currency baseCurrency,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            "api/account/preferences")
        {
            Content = JsonContent.Create(
                new
                {
                    Language = language,
                    BaseCurrency = baseCurrency
                },
                options: JsonOptions)
        };

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> DeleteAccountAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            "api/account");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
