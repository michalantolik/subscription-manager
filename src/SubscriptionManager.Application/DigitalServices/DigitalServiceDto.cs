using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices;

/// <summary>
/// Digital service data returned by digital service use cases.
/// </summary>
public sealed record DigitalServiceDto(
    Guid Id,
    string Key,
    string Name,
    bool IsPredefined,
    DigitalServiceCategory Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl,
    bool IsActive,
    int SortOrder);
