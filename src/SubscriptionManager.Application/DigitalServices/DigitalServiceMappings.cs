using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices;

/// <summary>
/// Maps digital service domain models to application DTOs.
/// </summary>
public static class DigitalServiceMappings
{
    public static DigitalServiceDto ToDto(
        this DigitalService digitalService)
    {
        return new DigitalServiceDto(
            digitalService.Id,
            digitalService.Key,
            digitalService.Name,
            digitalService.IsPredefined,
            digitalService.Category,
            digitalService.CustomCategoryName,
            digitalService.IconKey,
            digitalService.ManagementUrl,
            digitalService.IsActive,
            digitalService.SortOrder);
    }
}
