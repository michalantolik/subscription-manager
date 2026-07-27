using Moq;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.DigitalServices.UpdateDigitalService;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.Tests.DigitalServices.UpdateDigitalService;

public sealed class UpdateDigitalServiceHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldUpdateOwnedCustomDigitalService()
    {
        var ownerId = Guid.NewGuid();
        var service = DigitalService.CreateCustom(Guid.NewGuid(), ownerId,
            "old", "Old", DigitalServiceCategory.Other, null, null, null,
            DateTimeOffset.UtcNow);
        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(ownerId);
        repository.Setup(x => x.GetCustomByIdAsync(
            service.Id, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        var result = await new UpdateDigitalServiceHandler(
            repository.Object, currentUser.Object).HandleAsync(
            new UpdateDigitalServiceCommand(service.Id, "new", "New",
                DigitalServiceCategory.Productivity, null, null, null));

        Assert.True(result);
        Assert.Equal("new", service.Key);
        Assert.Equal("New", service.Name);
        repository.Verify(x => x.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalse_WhenCustomServiceDoesNotExist()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        var repository = new Mock<IDigitalServiceRepository>();
        var command = new UpdateDigitalServiceCommand(Guid.NewGuid(), "new", "New",
            DigitalServiceCategory.Productivity, null, null, null);

        var result = await new UpdateDigitalServiceHandler(
            repository.Object, currentUser.Object).HandleAsync(command);

        Assert.False(result);
        repository.Verify(x => x.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
