using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Infrastructure.Data;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

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

        _mockOrderingContext.Setup(x => x.GetCurrentTransaction()).Returns(new object());

        _service = new OrderingIntegrationEventService(
            _mockEventBus.Object,
            _mockOrderingContext.Object,
            _mockEventLogService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformationMessage_WithValidEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var integrationEvent = new OrderStartedIntegrationEvent("test-user") { Id = eventId };

        // Act
        await _service.AddAndSaveEventAsync(integrationEvent);

        // Assert - Verifies the LogInformation call on line 38
        _mockLogger.Verify(
            x => x.LogInformation(
                "Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})",
                eventId,
                integrationEvent),
            Times.Once);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_CallsSaveEventAsync_OnEventLogService()
    {
        // Arrange
        var integrationEvent = new OrderStartedIntegrationEvent("test-user");

        // Act
        await _service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        _mockEventLogService.Verify(
            x => x.SaveEventAsync(
                integrationEvent,
                It.IsAny<object>()),
            Times.Once);
    }
}
