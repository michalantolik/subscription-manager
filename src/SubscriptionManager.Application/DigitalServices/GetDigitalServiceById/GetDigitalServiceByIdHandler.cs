using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.DigitalServices.GetDigitalServiceById;

public sealed class GetDigitalServiceByIdHandler
{
    private readonly IDigitalServiceRepository _digitalServiceRepository;
    private readonly ICurrentUser _currentUser;

    public GetDigitalServiceByIdHandler(
        IDigitalServiceRepository digitalServiceRepository,
        ICurrentUser currentUser)
    {
        _digitalServiceRepository = digitalServiceRepository;
        _currentUser = currentUser;
    }

    public async Task<DigitalServiceDto?> HandleAsync(
        Guid digitalServiceId,
        CancellationToken cancellationToken = default)
    {
        var digitalService = await _digitalServiceRepository.GetAvailableByIdAsync(
            digitalServiceId,
            _currentUser.UserId,
            cancellationToken);

        return digitalService?.ToDto();
    }
}
