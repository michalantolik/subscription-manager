using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices.UpdateDigitalService;

/// <summary>
/// Request to update a digital service.
/// </summary>
public sealed record UpdateDigitalServiceCommand(
    Guid DigitalServiceId,
    string Key,
    string Name,
    DigitalServiceCategory Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl);
