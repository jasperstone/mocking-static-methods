using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

namespace eShop.Ordering.API.Tests.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    private readonly Mock<ILogger<OrderingIntegrationEventService>> _mockLogger;
    private readonly OrderingIntegrationEventService _service;

    public OrderingIntegrationEventServiceTests()
    {
        _mockLogger = new();

        // Create minimal mocks that don't require missing types
        var mockEventBus = new Mock<IEventBus>().Object;
        var mockOrderingContext = Mock.Of<OrderingContext>();
        var mockEventLogService = Mock.Of<IIntegrationEventLogService>();

        _service = new OrderingIntegrationEventService(
            mockEventBus,
            mockOrderingContext,
            mockEventLogService,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformationMessage()
    {
        // Arrange
        var evt = new OrderStartedIntegrationEvent("user123");

        // Act
        await _service.AddAndSaveEventAsync(evt);

        // Assert - Verifies the LogInformation call on line 38
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(formatter =>
                    formatter(It.IsAny<It.IsAnyType>(), null)?.Contains("Enqueuing integration event") == true &&
                    formatter(It.IsAny<It.IsAnyType>(), null)?.Contains(evt.Id.ToString()) == true)),
            Times.Once);
    }
}
