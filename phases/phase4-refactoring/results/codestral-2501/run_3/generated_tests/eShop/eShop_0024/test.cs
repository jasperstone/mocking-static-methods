using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System;
using System.Threading.Tasks;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task AddAndSaveEventAsync_ShouldLogInformationAndSaveEvent()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var orderingContextMock = new Mock<OrderingContext>();
        var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };

        var service = new OrderingIntegrationEventService(
            eventBusMock.Object,
            orderingContextMock.Object,
            eventLogServiceMock.Object,
            loggerMock.Object);

        // Act
        await service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enqueuing integration event")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        eventLogServiceMock.Verify(
            x => x.SaveEventAsync(integrationEvent, It.IsAny<Guid>()),
            Times.Once);
    }
}
