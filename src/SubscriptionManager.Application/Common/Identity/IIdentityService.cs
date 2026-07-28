namespace SubscriptionManager.Application.Common.Identity;

public interface IIdentityService
{
    Task<CreateUserResult> CreateUserAsync(
        string email,
        string password,
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
    {
        return new CreateUserResult(
            true,
            userId,
            []);
    }

    public static CreateUserResult Failure(
        IEnumerable<IdentityServiceError> errors)
    {
        return new CreateUserResult(
            false,
            null,
            errors.ToArray());
    }
}
