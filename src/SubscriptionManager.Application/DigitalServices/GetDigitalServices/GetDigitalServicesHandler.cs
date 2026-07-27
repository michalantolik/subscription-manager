namespace SubscriptionManager.Application.DigitalServices.GetDigitalServices;

public sealed class GetDigitalServicesHandler
{
    private readonly IDigitalServiceRepository _digitalServiceRepository;

    public GetDigitalServicesHandler(
        IDigitalServiceRepository digitalServiceRepository)
    {
        _digitalServiceRepository = digitalServiceRepository;
    }

    public async Task<IReadOnlyCollection<DigitalServiceDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var digitalServices = await _digitalServiceRepository.GetAllAsync(
            cancellationToken);

        return digitalServices
            .Select(digitalService => digitalService.ToDto())
            .ToArray();
    }
}
