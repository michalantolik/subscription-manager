using System.Security.Claims;
using SubscriptionManager.Web.Features.Authentication.Security;

namespace SubscriptionManager.Web.Features.DigitalServices;

/// <summary>
/// Provides access to digital service-related API operations.
/// </summary>
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

    public async Task<Guid> CreateAsync(
        CreateDigitalServiceFormModel model,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/digital-services")
        {
            Content = JsonContent.Create(
                new
                {
                    Key = $"custom-{Guid.NewGuid():N}",
                    Name = model.Name.Trim(),
                    Category = "Other",
                    CustomCategoryName = model.Category.Trim(),
                    IconKey = (string?)null,
                    ManagementUrl =
                        string.IsNullOrWhiteSpace(model.ManagementUrl)
                            ? null
                            : model.ManagementUrl.Trim()
                })
        };

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(
            cancellationToken);
    }
}
