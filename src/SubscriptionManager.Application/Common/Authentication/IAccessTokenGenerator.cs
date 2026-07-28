namespace SubscriptionManager.Application.Common.Authentication;

public interface IAccessTokenGenerator
{
    string GenerateToken(Guid userId);
}
