using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

namespace eShop.Ordering.API.Application.IntegrationEvents.Tests;

public class OrderingIntegrationEventServiceTests
{
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<OrderingContext> _mockOrderingContext;
    private readonly Mock<IIntegrationEventLogService> _mockEventLogService;
    private readonly Mock<ILogger<OrderingIntegrationEventService>> _mockLogger;
    private readonly OrderingIntegrationEventService _service;

    public OrderingIntegrationEventServiceTests()
    {
        _mockEventBus = new();
        _mockOrderingContext = new();
        _mockEventLogService = new();
        _mockLogger = new();

        _service = new OrderingIntegrationEventService(
            _mockEventBus.Object,
            _mockOrderingContext.Object,
            _mockEventLogService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformationMessage()
    {
        // Arrange
        var integrationEvent = new OrderStartedIntegrationEvent("user123");
        _mockEventLogService.Setup(x => x.SaveEventAsync(It.IsAny<IntegrationEvent>(), It.IsAny<Guid>()));

        // Act
        await _service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing integration event")),
                It.IsAny<IntegrationEvent>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WithPendingEvents_LogsPublishingMessage()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var integrationEvent = new OrderStartedIntegrationEvent("user123");
        
        // Mock IntegrationEventLogEntry - assuming it has EventId and IntegrationEvent properties
        var logEntryMock = new { EventId = eventId, IntegrationEvent = integrationEvent };
        
        _mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new[] { logEntryMock });

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing integration event")),
                It.IsAny<object>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
