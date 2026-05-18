using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.Ordering.Infrastructure;
using eShop.IntegrationEventLogEF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<OrderingContext> _mockOrderingContext;
    private readonly Mock<IIntegrationEventLogService> _mockEventLogService;
    private readonly Mock<ILogger<OrderingIntegrationEventService>> _mockLogger;

    public OrderingIntegrationEventServiceTests()
    {
        _mockEventBus = new Mock<IEventBus>();
        _mockOrderingContext = new Mock<OrderingContext>();
        _mockEventLogService = new Mock<IIntegrationEventLogService>();
        _mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WhenPendingEventsExist_ShouldLogInformationForEachEvent()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var integrationEvent1 = new TestIntegrationEvent { Id = eventId1 };
        var integrationEvent2 = new TestIntegrationEvent { Id = eventId2 };

        var pendingLogEvents = new List<IntegrationEventLogEntry>
        {
            new() { EventId = eventId1, IntegrationEvent = integrationEvent1 },
            new() { EventId = eventId2, IntegrationEvent = integrationEvent2 }
        };

        _mockEventLogService
            .Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(pendingLogEvents);

        // Setup mocks to avoid exceptions during execution
        _mockEventLogService
            .Setup(x => x.MarkEventAsInProgressAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _mockEventBus
            .Setup(x => x.PublishAsync(It.IsAny<IntegrationEvent>()))
            .Returns(Task.CompletedTask);
        _mockEventLogService
            .Setup(x => x.MarkEventAsPublishedAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockOrderingContext
            .Setup(x => x.GetCurrentTransaction())
            .Returns((IDbContextTransaction)null);

        var service = new OrderingIntegrationEventService(
            _mockEventBus.Object,
            _mockOrderingContext.Object,
            _mockEventLogService.Object,
            _mockLogger.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert - Verify LogInformation was called for each event (line 19)
        _mockLogger.Verify(
            x => x.LogInformation(
                "Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})",
                eventId1,
                integrationEvent1),
            Times.Once);

        _mockLogger.Verify(
            x => x.LogInformation(
                "Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})",
                eventId2,
                integrationEvent2),
            Times.Once);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WhenNoPendingEvents_ShouldNotLogInformation()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        _mockEventLogService
            .Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<IntegrationEventLogEntry>());

        var service = new OrderingIntegrationEventService(
            _mockEventBus.Object,
            _mockOrderingContext.Object,
            _mockEventLogService.Object,
            _mockLogger.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert - Verify LogInformation was not called
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.Contains("Publishing integration event")),
                It.IsAny<object[]>()
            ),
            Times.Never);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_ShouldLogInformation()
    {
        // Arrange
        var evt = new TestIntegrationEvent { Id = Guid.NewGuid() };
        _mockEventLogService.Setup(x => x.SaveEventAsync(It.IsAny<IntegrationEvent>(), It.IsAny<IDbContextTransaction>()))
            .Returns(Task.CompletedTask);
        _mockOrderingContext.Setup(x => x.GetCurrentTransaction()).Returns((IDbContextTransaction)null);

        var service = new OrderingIntegrationEventService(
            _mockEventBus.Object,
            _mockOrderingContext.Object,
            _mockEventLogService.Object,
            _mockLogger.Object);

        // Act
        await service.AddAndSaveEventAsync(evt);

        // Assert - Verify LogInformation was called
        _mockLogger.Verify(
            x => x.LogInformation(
                "Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})",
                evt.Id,
                evt),
            Times.Once);
    }
}

// Test classes to satisfy compilation
public class TestIntegrationEvent : IntegrationEvent
{
    public TestIntegrationEvent()
    {
        Id = Guid.NewGuid();
    }
}

public class IntegrationEventLogEntry
{
    public Guid EventId { get; set; }
    public IntegrationEvent IntegrationEvent { get; set; }
}

public partial class OrderingContext
{
    public virtual IDbContextTransaction GetCurrentTransaction() => null;
}
