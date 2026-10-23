using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using eShop.Ordering.API.Application.IntegrationEvents;

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
        _mockEventBus = new Mock<IEventBus>();
        _mockOrderingContext = new Mock<OrderingContext>();
        _mockEventLogService = new Mock<IIntegrationEventLogService>();
        _mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

        _mockOrderingContext.Setup(x => x.GetCurrentTransaction()).Returns(Mock.Of<IDbContextTransaction>());

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
        var eventId = Guid.NewGuid();
        var integrationEvent = new Mock<IntegrationEvent>();
        integrationEvent.Setup(e => e.Id).Returns(eventId);

        _mockEventLogService.Setup(x => x.SaveEventAsync(It.IsAny<IntegrationEvent>(), It.IsAny<IDbContextTransaction>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddAndSaveEventAsync(integrationEvent.Object);

        // Assert - Verifies the LogInformation call on line 38
        _mockLogger.Verify(
            x => x.LogInformation(
                "Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})",
                eventId,
                integrationEvent.Object),
            Times.Once);
    }
}
