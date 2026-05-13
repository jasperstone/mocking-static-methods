using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;

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
            var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, integrationEventLogServiceMock.Object, loggerMock.Object);

            var integrationEventLog = new IntegrationEventLog { EventId = Guid.NewGuid(), IntegrationEvent = new IntegrationEvent { Id = Guid.NewGuid() } };
            integrationEventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>())).ReturnsAsync(new List<IntegrationEventLog> { integrationEventLog });

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
            var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, integrationEventLogServiceMock.Object, loggerMock.Object);

            var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
