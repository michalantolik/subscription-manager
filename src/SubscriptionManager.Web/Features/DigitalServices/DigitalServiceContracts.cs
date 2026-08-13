namespace SubscriptionManager.Web.Features.DigitalServices;

/// <summary>
/// Digital service data returned by the Digital Services API.
/// </summary>
public sealed record DigitalServiceResponse(
    Guid Id,
    string Key,
    string Name,
    string Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl,
    bool IsPredefined);
