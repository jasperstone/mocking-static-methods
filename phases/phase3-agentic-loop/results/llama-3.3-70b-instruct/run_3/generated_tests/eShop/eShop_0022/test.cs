using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Infrastructure;

namespace eShop.Ordering.API.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<eShop.Ordering.API.Application.IntegrationEvents.IEventBus>();
            var orderingContextMock = new Mock<eShop.Ordering.API.Infrastructure.OrderingContext>();
            var integrationEventLogServiceMock = new Mock<eShop.Ordering.API.Application.IntegrationEvents.IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, integrationEventLogServiceMock.Object, loggerMock.Object);

            var transactionId = Guid.NewGuid();
            var logEvt = new IntegrationEventLog { EventId = Guid.NewGuid(), IntegrationEvent = new IntegrationEvent { Id = Guid.NewGuid() } };

            integrationEventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId)).ReturnsAsync(new[] { logEvt });

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Publishing integration event:")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<eShop.Ordering.API.Application.IntegrationEvents.IEventBus>();
            var orderingContextMock = new Mock<eShop.Ordering.API.Infrastructure.OrderingContext>();
            var integrationEventLogServiceMock = new Mock<eShop.Ordering.API.Application.IntegrationEvents.IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, integrationEventLogServiceMock.Object, loggerMock.Object);

            var evt = new IntegrationEvent { Id = Guid.NewGuid() };

            // Act
            await service.AddAndSaveEventAsync(evt);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Enqueuing integration event:")), It.IsAny<object[]>()), Times.Once);
        }
    }
}
