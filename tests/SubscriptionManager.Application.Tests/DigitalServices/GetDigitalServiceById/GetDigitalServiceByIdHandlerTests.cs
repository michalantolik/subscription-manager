using Moq;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.DigitalServices.GetDigitalServiceById;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.Tests.DigitalServices.GetDigitalServiceById;

public sealed class GetDigitalServiceByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnAvailableDigitalService()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var service = DigitalService.CreateCustom(id, ownerId, "my-service",
            "My Service", DigitalServiceCategory.Other, null, null, null,
            DateTimeOffset.UtcNow);
        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(ownerId);
        repository.Setup(x => x.GetAvailableByIdAsync(
            id, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        var result = await new GetDigitalServiceByIdHandler(
            repository.Object, currentUser.Object).HandleAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenDigitalServiceIsNotAvailable()
    {
        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

        var result = await new GetDigitalServiceByIdHandler(
            repository.Object, currentUser.Object).HandleAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
