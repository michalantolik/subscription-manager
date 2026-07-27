using System.Net.Http.Json;

namespace SubscriptionManager.Blazor.Features.DigitalServices;

public sealed record DigitalServiceResponse(
    Guid Id,
    string Key,
    string Name,
    string Category,
    string? IconKey,
    string? ManagementUrl,
    bool IsPredefined);

public sealed class DigitalServicesApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<DigitalServiceResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyList<DigitalServiceResponse>>(
                   "api/digital-services",
                   cancellationToken)
               ?? [];
    }
}
