using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

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
    public async Task AddAndSaveEventAsync_ShouldLogInformation()
    {
        // Arrange
        var integrationEvent = new OrderStartedIntegrationEvent("user1");

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
