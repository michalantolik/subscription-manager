using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.DigitalServices.UpdateDigitalService;

/// <summary>
/// Handles digital service update.
/// </summary>
public sealed class UpdateDigitalServiceHandler
{
    private readonly IDigitalServiceRepository _digitalServiceRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateDigitalServiceHandler(
        IDigitalServiceRepository digitalServiceRepository,
        ICurrentUser currentUser)
    {
        _digitalServiceRepository = digitalServiceRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> HandleAsync(
        UpdateDigitalServiceCommand command,
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

        digitalService.Update(
            command.Key,
            command.Name,
            command.Category,
            command.CustomCategoryName,
            command.IconKey,
            command.ManagementUrl);

        await _digitalServiceRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
