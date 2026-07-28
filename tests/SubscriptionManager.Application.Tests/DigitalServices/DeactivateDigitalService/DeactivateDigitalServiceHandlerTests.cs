using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.DigitalServices.DeactivateDigitalService;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.Tests.DigitalServices.DeactivateDigitalService;

public sealed class DeactivateDigitalServiceHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldDeactivateCurrentUserCustomDigitalService_WhenDigitalServiceExists()
    {
        var ownerId = Guid.NewGuid();
        var digitalService = DigitalService.CreateCustom(
            Guid.NewGuid(),
            ownerId,
            "my-service",
            "My Service",
            DigitalServiceCategory.Other,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);

        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        repository
            .Setup(x => x.GetCustomByIdAsync(
                digitalService.Id,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(digitalService);

        var handler = new DeactivateDigitalServiceHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new DeactivateDigitalServiceCommand(digitalService.Id));

        Assert.True(result);
        Assert.False(digitalService.IsActive);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalse_WhenCurrentUserCustomDigitalServiceDoesNotExist()
    {
        var digitalServiceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        repository
            .Setup(x => x.GetCustomByIdAsync(
                digitalServiceId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DigitalService?)null);

        var handler = new DeactivateDigitalServiceHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new DeactivateDigitalServiceCommand(digitalServiceId));

        Assert.False(result);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var handler = new DeactivateDigitalServiceHandler(
            repository.Object,
            currentUser.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!));

        repository.Verify(
            x => x.GetCustomByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
