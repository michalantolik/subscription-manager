namespace SubscriptionManager.Application.DigitalServices.DeactivateDigitalService;

/// <summary>
/// Request to deactivate a digital service.
/// </summary>
public sealed record DeactivateDigitalServiceCommand(Guid DigitalServiceId);
