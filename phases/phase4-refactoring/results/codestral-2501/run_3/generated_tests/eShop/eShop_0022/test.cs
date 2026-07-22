using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformation()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var orderingContextMock = new Mock<OrderingContext>();
        var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        var transactionId = Guid.NewGuid();
        var pendingLogEvents = new List<IntegrationEventLogEntry>
        {
            new IntegrationEventLogEntry { EventId = Guid.NewGuid(), IntegrationEvent = new IntegrationEvent() }
        };

        integrationEventLogServiceMock
            .Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(pendingLogEvents);

        var service = new OrderingIntegrationEventService(
            eventBusMock.Object,
            orderingContextMock.Object,
            integrationEventLogServiceMock.Object,
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
