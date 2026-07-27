using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices.CreateDigitalService;

public sealed record CreateDigitalServiceCommand(
    string Key,
    string Name,
    DigitalServiceCategory Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl);
