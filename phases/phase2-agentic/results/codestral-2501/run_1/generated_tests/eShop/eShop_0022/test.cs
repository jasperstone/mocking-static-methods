using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<OrderingContext> _orderingContextMock;
    private readonly Mock<IIntegrationEventLogService> _integrationEventLogServiceMock;
    private readonly Mock<ILogger<OrderingIntegrationEventService>> _loggerMock;
    private readonly OrderingIntegrationEventService _service;

    public OrderingIntegrationEventServiceTests()
    {
        _eventBusMock = new Mock<IEventBus>();
        _orderingContextMock = new Mock<OrderingContext>();
        _integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
        _loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        _service = new OrderingIntegrationEventService(
            _eventBusMock.Object,
            _orderingContextMock.Object,
            _integrationEventLogServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformation()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var logEvent = new IntegrationEventLogEntry
        {
            EventId = Guid.NewGuid(),
            IntegrationEvent = new IntegrationEvent()
        };

        _integrationEventLogServiceMock
            .Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<IntegrationEventLogEntry> { logEvent });

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publishing integration event")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
