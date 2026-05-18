using Xunit;
using Moq;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

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

            var transactionId = Guid.NewGuid();
            var pendingLogEvents = new[] { new IntegrationEventLog { EventId = Guid.NewGuid(), IntegrationEvent = new TestIntegrationEvent() } };

            eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId)).ReturnsAsync(pendingLogEvents);

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

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

            var @event = new TestIntegrationEvent();

            // Act
            await service.AddAndSaveEventAsync(@event);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        private class TestIntegrationEvent : IntegrationEvent
        {
        }
    }
}
