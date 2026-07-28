namespace SubscriptionManager.Application.Common.Identity;

public interface IIdentityService
{
    Task<CreateUserResult> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<string?> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ConfirmEmailResult> ConfirmEmailAsync(
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken = default);
}

public sealed record IdentityServiceError(
    string Code,
    string Description);

public sealed record CreateUserResult(
    bool Succeeded,
    Guid? UserId,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static CreateUserResult Success(Guid userId)
        => new(true, userId, []);

    public static CreateUserResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(false, null, errors.ToArray());
}

public sealed record ConfirmEmailResult(
    bool Succeeded,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static ConfirmEmailResult Success()
        => new(true, []);

    public static ConfirmEmailResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(false, errors.ToArray());
}
