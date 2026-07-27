using SubscriptionManager.Application.Common.Authentication;

namespace SubscriptionManager.Application.DigitalServices.DeactivateDigitalService;

public sealed class DeactivateDigitalServiceHandler
{
    private readonly IDigitalServiceRepository _digitalServiceRepository;
    private readonly ICurrentUser _currentUser;

    public DeactivateDigitalServiceHandler(
        IDigitalServiceRepository digitalServiceRepository,
        ICurrentUser currentUser)
    {
        _digitalServiceRepository = digitalServiceRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> HandleAsync(
        DeactivateDigitalServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var digitalService = await _digitalServiceRepository.GetCustomByIdAsync(
            command.DigitalServiceId,
            _currentUser.UserId,
            cancellationToken);

        if (digitalService is null)
        {
            return false;
        }

        digitalService.Deactivate();
        await _digitalServiceRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
