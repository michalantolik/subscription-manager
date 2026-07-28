using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.DigitalServices.CreateDigitalService;

public sealed class CreateDigitalServiceHandler
{
    private readonly IDigitalServiceRepository _digitalServiceRepository;
    private readonly ICurrentUser _currentUser;

    public CreateDigitalServiceHandler(
        IDigitalServiceRepository digitalServiceRepository,
        ICurrentUser currentUser)
    {
        _digitalServiceRepository = digitalServiceRepository;
        _currentUser = currentUser;
    }

    public async Task<Guid> HandleAsync(
        CreateDigitalServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var digitalService = DigitalService.CreateCustom(
            Guid.NewGuid(),
            _currentUser.UserId,
            command.Key,
            command.Name,
            command.Category,
            command.CustomCategoryName,
            command.IconKey,
            command.ManagementUrl,
            DateTimeOffset.UtcNow);

        await _digitalServiceRepository.AddAsync(
            digitalService,
            cancellationToken);
        await _digitalServiceRepository.SaveChangesAsync(cancellationToken);

        return digitalService.Id;
    }
}
