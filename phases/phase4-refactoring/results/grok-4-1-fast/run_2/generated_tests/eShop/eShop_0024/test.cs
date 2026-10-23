using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Infrastructure;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents;

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
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockOrderingContext.Setup(x => x.GetCurrentTransaction()).Returns(mockTransaction.Object);
        _mockEventLogService.Setup(x => x.SaveEventAsync(It.IsAny<IntegrationEvent>(), It.IsAny<IDbContextTransaction>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Enqueuing integration event") && 
                    v.ToString()!.Contains(integrationEvent.Id.ToString())),
                It.IsAny<IntegrationEvent>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_NoPendingEvents_DoesNotLogPublishing()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        _mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<IntegrationEventLogEntry>());

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing integration event")),
                It.IsAny<object[]>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WithPendingEvents_LogsInformationForEach()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var logEvents = new List<IntegrationEventLogEntry>
        {
            new() { EventId = eventId1, IntegrationEvent = new OrderStartedIntegrationEvent("user1") },
            new() { EventId = eventId2, IntegrationEvent = new OrderStartedIntegrationEvent("user2") }
        };

        _mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(logEvents);
        _mockEventLogService.Setup(x => x.MarkEventAsInProgressAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _mockEventBus.Setup(x => x.PublishAsync(It.IsAny<IntegrationEvent>())).Returns(Task.CompletedTask);
        _mockEventLogService.Setup(x => x.MarkEventAsPublishedAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing integration event")),
                It.IsAny<object[]>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_PublishFails_LogsError()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var logEvent = new IntegrationEventLogEntry 
        { 
            EventId = eventId, 
            IntegrationEvent = new OrderStartedIntegrationEvent("user1") 
        };
        _mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<IntegrationEventLogEntry> { logEvent });
        _mockEventLogService.Setup(x => x.MarkEventAsInProgressAsync(eventId)).Returns(Task.CompletedTask);
        _mockEventBus.Setup(x => x.PublishAsync(It.IsAny<IntegrationEvent>()))
            .ThrowsAsync(new InvalidOperationException("Publish failed"));
        _mockEventLogService.Setup(x => x.MarkEventAsFailedAsync(eventId)).Returns(Task.CompletedTask);

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error publishing integration event")),
                It.IsAny<object[]>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

// Test doubles for missing types
public interface IEventBus
{
    Task PublishAsync(IntegrationEvent @event);
}

public interface IIntegrationEventLogService
{
    Task<List<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
    Task MarkEventAsInProgressAsync(Guid eventId);
    Task MarkEventAsPublishedAsync(Guid eventId);
    Task MarkEventAsFailedAsync(Guid eventId);
    Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);
}

public class IntegrationEventLogEntry
{
    public Guid EventId { get; set; }
    public IntegrationEvent IntegrationEvent { get; set; } = null!;
}
