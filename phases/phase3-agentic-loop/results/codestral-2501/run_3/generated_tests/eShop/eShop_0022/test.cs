using Xunit;
using Moq;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public async Task PublishEventsThroughEventBusAsync_ShouldLogInformation_WhenEventsArePublished()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var integrationEvent = new OrderStartedIntegrationEvent(transactionId);
        var eventLogEntry = new EventLogEntry { EventId = Guid.NewGuid(), IntegrationEvent = integrationEvent };
        _integrationEventLogServiceMock.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<EventLogEntry> { eventLogEntry });

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_ShouldMarkEventAsPublished_WhenEventsArePublished()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var integrationEvent = new OrderStartedIntegrationEvent(transactionId);
        var eventLogEntry = new EventLogEntry { EventId = Guid.NewGuid(), IntegrationEvent = integrationEvent };
        _integrationEventLogServiceMock.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<EventLogEntry> { eventLogEntry });

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        _integrationEventLogServiceMock.Verify(x => x.MarkEventAsPublishedAsync(eventLogEntry.EventId), Times.Once);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var integrationEvent = new OrderStartedIntegrationEvent(transactionId);
        var eventLogEntry = new EventLogEntry { EventId = Guid.NewGuid(), IntegrationEvent = integrationEvent };
        _integrationEventLogServiceMock.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<EventLogEntry> { eventLogEntry });
        _eventBusMock.Setup(x => x.PublishAsync(integrationEvent)).ThrowsAsync(new Exception("Test exception"));

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_ShouldLogInformation_WhenEventIsAdded()
    {
        // Arrange
        var integrationEvent = new OrderStartedIntegrationEvent(Guid.NewGuid());

        // Act
        await _service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
