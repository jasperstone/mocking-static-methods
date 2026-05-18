using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Ordering.API.Tests.Application.IntegrationEvents;

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
    public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachPendingEvent()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var dummyEvent1 = new object();
        var dummyEvent2 = new object();

        var pendingEvents = new List<object>
        {
            new { EventId = eventId1, IntegrationEvent = dummyEvent1 },
            new { EventId = eventId2, IntegrationEvent = dummyEvent2 }
        };

        _mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(pendingEvents);

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert - Verify LogInformation was called for each event (line 19 coverage)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing integration event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformationEventEnqueued()
    {
        // Arrange
        var dummyEvent = new object();

        // Act
        await _service.AddAndSaveEventAsync((IntegrationEvent)dummyEvent);

        // Assert - Verify LogInformation call in AddAndSaveEventAsync
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing integration event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());
    }
}
