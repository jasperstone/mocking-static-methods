using Xunit;
using Moq;
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
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var eventBusMock = new Mock<IEventBus>();
            var orderingContextMock = new Mock<OrderingContext>();
            var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();

            var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };
            var integrationEventLog = new IntegrationEventLog { EventId = integrationEvent.Id, IntegrationEvent = integrationEvent };

            integrationEventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new[] { integrationEventLog });

            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, integrationEventLogServiceMock.Object, loggerMock.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Publishing integration event"))), Times.Once);
        }

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var eventBusMock = new Mock<IEventBus>();
            var orderingContextMock = new Mock<OrderingContext>();
            var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();

            var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };

            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, integrationEventLogServiceMock.Object, loggerMock.Object);

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Enqueuing integration event"))), Times.Once);
        }
    }
}
