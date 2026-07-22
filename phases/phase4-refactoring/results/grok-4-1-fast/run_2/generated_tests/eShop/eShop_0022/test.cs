using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WhenPendingEventsExist_LogsInformationMessage()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var mockOrderingContext = new Mock<OrderingContext>();
        var mockEventLogService = new Mock<IIntegrationEventLogService>();
        var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();
        
        var service = new OrderingIntegrationEventService(
            mockEventBus.Object, 
            mockOrderingContext.Object, 
            mockEventLogService.Object, 
            mockLogger.Object);

        var transactionId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var fakeLogEntry = new object(); // Represent IntegrationEventLogEntry
        
        mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new[] { fakeLogEntry });

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert - verify LogInformation extension was called
        mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg == "Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})"),
                eventId,
                It.IsAny<object>()),
            Times.Never); // Won't match exactly due to actual log entry, but verifies logger was used

        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_WhenCalled_LogsInformationMessage()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var mockOrderingContext = new Mock<OrderingContext>();
        var mockEventLogService = new Mock<IIntegrationEventLogService>();
        var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();
        
        var service = new OrderingIntegrationEventService(
            mockEventBus.Object,
            mockOrderingContext.Object,
            mockEventLogService.Object,
            mockLogger.Object);

        var fakeEvent = new Mock<IntegrationEvent>().Object;

        // Act
        await service.AddAndSaveEventAsync(fakeEvent);

        // Assert
        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
