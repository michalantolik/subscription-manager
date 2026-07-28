using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.DigitalServices.GetDigitalServices;

public sealed class GetDigitalServicesHandler
{
    private readonly IDigitalServiceRepository _digitalServiceRepository;
    private readonly ICurrentUser _currentUser;

    public GetDigitalServicesHandler(
        IDigitalServiceRepository digitalServiceRepository,
        ICurrentUser currentUser)
    {
        _digitalServiceRepository = digitalServiceRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<DigitalServiceDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var digitalServices =
            await _digitalServiceRepository.GetAvailableAsync(
                _currentUser.UserId,
                cancellationToken);

        return digitalServices
            .Select(digitalService => digitalService.ToDto())
            .ToArray();
    }
}
