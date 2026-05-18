using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderingIntegrationEventServiceTests
{
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<OrderingContext> _orderingContextMock;
    private readonly Mock<IIntegrationEventLogService> _eventLogServiceMock;
    private readonly Mock<ILogger<OrderingIntegrationEventService>> _loggerMock;
    private readonly OrderingIntegrationEventService _service;

    public OrderingIntegrationEventServiceTests()
    {
        _eventBusMock = new Mock<IEventBus>();
        _orderingContextMock = new Mock<OrderingContext>();
        _eventLogServiceMock = new Mock<IIntegrationEventLogService>();
        _loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        _service = new OrderingIntegrationEventService(
            _eventBusMock.Object,
            _orderingContextMock.Object,
            _eventLogServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformation()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var logEvt = new IntegrationEventLogEntry
        {
            EventId = Guid.NewGuid(),
            IntegrationEvent = new OrderStartedIntegrationEvent("user1")
        };

        _eventLogServiceMock.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<IntegrationEventLogEntry> { logEvt });

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

    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformation()
    {
        // Arrange
        var evt = new OrderStartedIntegrationEvent("user1");

        // Act
        await _service.AddAndSaveEventAsync(evt);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enqueuing integration event")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
