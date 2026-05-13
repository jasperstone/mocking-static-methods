using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System.Threading.Tasks;
using System;

namespace eShop.Ordering.API.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<IEventBus>();
            var orderingContextMock = new Mock<OrderingContext>();
            var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

            var pendingLogEvents = new[] { new IntegrationEventLog() };
            eventLogServiceMock.Setup(es => es.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>())).ReturnsAsync(pendingLogEvents);

            // Act
            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<IEventBus>();
            var orderingContextMock = new Mock<OrderingContext>();
            var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

            var integrationEvent = new IntegrationEvent();
            eventLogServiceMock.Setup(es => es.SaveEventAsync(It.IsAny<IntegrationEvent>(), It.IsAny<object>())).ReturnsAsync(true);

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
