namespace SubscriptionManager.Application.DigitalServices.DeleteDigitalService;

/// <summary>
/// Request to delete a digital service.
/// </summary>
public sealed record DeleteDigitalServiceCommand(Guid DigitalServiceId);
