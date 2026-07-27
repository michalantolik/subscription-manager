using SubscriptionManager.Application.Common.Authentication;

namespace SubscriptionManager.Application.DigitalServices.DeleteDigitalService;

public sealed class DeleteDigitalServiceHandler
{
    private readonly IDigitalServiceRepository _digitalServiceRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteDigitalServiceHandler(
        IDigitalServiceRepository digitalServiceRepository,
        ICurrentUser currentUser)
    {
        _digitalServiceRepository = digitalServiceRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> HandleAsync(
        DeleteDigitalServiceCommand command,
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

        _digitalServiceRepository.Remove(digitalService);
        await _digitalServiceRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
