using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformation()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var orderingContextMock = new Mock<OrderingContext>();
        var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        var transactionId = Guid.NewGuid();
        var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid(), EventId = Guid.NewGuid() };
        var logEvent = new IntegrationEventLogEntry { EventId = integrationEvent.EventId, IntegrationEvent = integrationEvent };

        eventLogServiceMock.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<IntegrationEventLogEntry> { logEvent });

        var service = new OrderingIntegrationEventService(
            eventBusMock.Object,
            orderingContextMock.Object,
            eventLogServiceMock.Object,
            loggerMock.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publishing integration event")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
