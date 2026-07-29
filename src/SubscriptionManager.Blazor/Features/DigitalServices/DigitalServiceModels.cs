using System.Net.Http.Json;
using System.Security.Claims;
using SubscriptionManager.Blazor.Features.Authentication;

namespace SubscriptionManager.Blazor.Features.DigitalServices;

public sealed record DigitalServiceResponse(
    Guid Id,
    string Key,
    string Name,
    string Category,
    string? IconKey,
    string? ManagementUrl,
    bool IsPredefined);

public sealed class DigitalServicesApiClient(
    HttpClient httpClient)
{
    public async Task<IReadOnlyList<DigitalServiceResponse>> GetAllAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/digital-services");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<IReadOnlyList<DigitalServiceResponse>>(
                       cancellationToken)
               ?? [];
    }
}
