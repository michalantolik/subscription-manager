using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using SubscriptionManager.Web.Features.Authentication;

namespace SubscriptionManager.Web.Features.DigitalServices;

public sealed record DigitalServiceResponse(
    Guid Id,
    string Key,
    string Name,
    string Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl,
    bool IsPredefined);

public sealed class CreateDigitalServiceFormModel
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Category { get; set; } = string.Empty;

    [Url]
    [StringLength(500)]
    public string? ManagementUrl { get; set; }
}

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
