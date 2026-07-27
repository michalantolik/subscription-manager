using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices.UpdateDigitalService;

public sealed record UpdateDigitalServiceCommand(
    Guid DigitalServiceId,
    string Key,
    string Name,
    DigitalServiceCategory Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl);
