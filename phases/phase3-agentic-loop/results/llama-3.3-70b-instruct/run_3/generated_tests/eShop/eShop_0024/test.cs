using Xunit;
using Moq;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.Domain.Core;
using eShop.Ordering.Infrastructure.Data.Interfaces;
using eShop.Ordering.Infrastructure.Bus.Interfaces;
using Microsoft.Extensions.Logging;

namespace eShop.Ordering.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<IEventBus>();
            var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var orderingContextMock = new Mock<OrderingContext>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

            var integrationEvent = new IntegrationEvent();
            eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>())).ReturnsAsync(new[] { new IntegrationEventLog { EventId = integrationEvent.Id, IntegrationEvent = integrationEvent } });

            // Act
            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Publishing integration event"))), Times.Once);
        }

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<IEventBus>();
            var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var orderingContextMock = new Mock<OrderingContext>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

            var integrationEvent = new IntegrationEvent();

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Enqueuing integration event"))), Times.Once);
        }
    }
}
