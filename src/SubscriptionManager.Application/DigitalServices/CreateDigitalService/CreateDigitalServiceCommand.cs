using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices.CreateDigitalService;

/// <summary>
/// Request to create a digital service.
/// </summary>
public sealed record CreateDigitalServiceCommand(
    string Key,
    string Name,
    DigitalServiceCategory Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl);
