using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<OrderingContext> _mockOrderingContext;
    private readonly Mock<IIntegrationEventLogService> _mockEventLogService;
    private readonly Mock<ILogger<OrderingIntegrationEventService>> _mockLogger;

    public OrderingIntegrationEventServiceTests()
    {
        _mockEventBus = new();
        _mockOrderingContext = new();
        _mockEventLogService = new();
        _mockLogger = new();
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WhenPendingEventsExist_LogsInformationMessage()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var integrationEvent = new TestIntegrationEvent { Id = eventId };
        var pendingLogEvents = new List<IntegrationEventLogEntry>
        {
            new() { EventId = eventId, IntegrationEvent = integrationEvent }
        };

        _mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(pendingLogEvents);
        _mockEventLogService.Setup(x => x.MarkEventAsInProgressAsync(eventId)).Returns(Task.CompletedTask);
        _mockEventBus.Setup(x => x.PublishAsync(integrationEvent)).Returns(Task.CompletedTask);
        _mockEventLogService.Setup(x => x.MarkEventAsPublishedAsync(eventId)).Returns(Task.CompletedTask);

        var service = new OrderingIntegrationEventService(
            _mockEventBus.Object,
            _mockOrderingContext.Object,
            _mockEventLogService.Object,
            _mockLogger.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert - Verify the LogInformation call on line 19
        _mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                "Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})",
                eventId,
                integrationEvent),
            Times.Once);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformationMessage()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var integrationEvent = new TestIntegrationEvent { Id = eventId };

        _mockOrderingContext.Setup(x => x.GetCurrentTransaction()).Returns((string)null);
        _mockEventLogService.Setup(x => x.SaveEventAsync(integrationEvent, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new OrderingIntegrationEventService(
            _mockEventBus.Object,
            _mockOrderingContext.Object,
            _mockEventLogService.Object,
            _mockLogger.Object);

        // Act
        await service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                "Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})",
                eventId,
                integrationEvent),
            Times.Once);
    }
}

// Minimal test doubles - fully self-contained
public abstract class IntegrationEvent
{
    public abstract Guid Id { get; }
}

public class TestIntegrationEvent : IntegrationEvent
{
    public override Guid Id { get; set; }
}

public class IntegrationEventLogEntry
{
    public Guid EventId { get; set; }
    public IntegrationEvent IntegrationEvent { get; set; } = null!;
}

public interface IEventBus
{
    Task PublishAsync(IntegrationEvent @event);
}

public class OrderingContext
{
    public virtual string? GetCurrentTransaction() => null;
}

public interface IIntegrationEventLogService
{
    Task<List<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
    Task SaveEventAsync(IntegrationEvent evt, string transactionId);
    Task MarkEventAsInProgressAsync(Guid eventId);
    Task MarkEventAsPublishedAsync(Guid eventId);
    Task MarkEventAsFailedAsync(Guid eventId);
}

public interface IOrderingIntegrationEventService
{
    Task PublishEventsThroughEventBusAsync(Guid transactionId);
    Task AddAndSaveEventAsync(IntegrationEvent evt);
}
