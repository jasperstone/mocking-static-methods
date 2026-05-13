using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Domain.Events;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachEvent()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var mockOrderingContext = new Mock<OrderingContext>();
        var mockEventLogService = new Mock<IIntegrationEventLogService>();
        var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

        var logEvents = new List<EventLog>
        {
            new EventLog { EventId = Guid.NewGuid(), IntegrationEvent = new IntegrationEvent(Guid.NewGuid()) }
        };

        mockEventLogService
            .Setup(service => service.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>()))
            .ReturnsAsync(logEvents);

        var service = new OrderingIntegrationEventService(
            mockEventBus.Object,
            mockOrderingContext.Object,
            mockEventLogService.Object,
            mockLogger.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

        // Assert
        foreach (var logEvent in logEvents)
        {
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains($"Publishing integration event: {logEvent.EventId}")),
                    It.Is<Dictionary<string, object>>(d => d.ContainsKey("IntegrationEventId") && d["IntegrationEventId"] == logEvent.EventId && d.ContainsKey("IntegrationEvent") && d["IntegrationEvent"] == logEvent.IntegrationEvent),
                    It.IsAny<ILoggerProvider>(),
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<Exception, ExceptionDispatchInfo>>()),
                Times.Once);
        }
    }
}
