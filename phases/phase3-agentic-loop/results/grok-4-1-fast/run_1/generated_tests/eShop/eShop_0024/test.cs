using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System;
using System.Threading.Tasks;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task AddAndSaveEventAsync_ValidEvent_LogsInformationMessage()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var mockOrderingContext = new Mock<OrderingContext>();
        var mockEventLogService = new Mock<IIntegrationEventLogService>();
        var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

        mockEventLogService.Setup(x => x.SaveEventAsync(It.IsAny<IntegrationEvent>(), It.IsAny<Guid?>()))
                          .Returns(Task.CompletedTask);

        var service = new OrderingIntegrationEventService(
            mockEventBus.Object,
            mockOrderingContext.Object,
            mockEventLogService.Object,
            mockLogger.Object);

        var testEvent = new Mock<IntegrationEvent>();
        testEvent.Setup(e => e.Id).Returns(Guid.NewGuid());

        // Act
        await service.AddAndSaveEventAsync(testEvent.Object);

        // Assert - Verify LogInformation was called
        mockLogger.Verify(
            x => x.LogInformation(
                "Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})",
                It.Is<Guid>(id => id == testEvent.Object.Id),
                It.IsAny<IntegrationEvent>()),
            Times.Once);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var mockOrderingContext = new Mock<OrderingContext>();
        var mockEventLogService = new Mock<IIntegrationEventLogService>();
        var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

        mockEventLogService.Setup(x => x.SaveEventAsync(It.IsAny<IntegrationEvent>(), It.IsAny<Guid?>()))
                          .Returns(Task.CompletedTask);

        var service = new OrderingIntegrationEventService(
            mockEventBus.Object,
            mockOrderingContext.Object,
            mockEventLogService.Object,
            mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAndSaveEventAsync(null));
    }
}
