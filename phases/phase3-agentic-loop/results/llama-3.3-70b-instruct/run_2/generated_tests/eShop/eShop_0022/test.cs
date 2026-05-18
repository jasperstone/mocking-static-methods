using Xunit;
using Moq;
using eShop.Ordering.API.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace eShop.Ordering.API.Application.Tests
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
            var pendingLogEvents = new[] { new eShop.Ordering.API.Application.IntegrationEvents.IntegrationEventLog { EventId = Guid.NewGuid(), IntegrationEvent = new eShop.Ordering.API.Application.IntegrationEvents.IntegrationEvent { Id = Guid.NewGuid() } } };

            integrationEventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId)).ReturnsAsync(pendingLogEvents);

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
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

            var integrationEvent = new eShop.Ordering.API.Application.IntegrationEvents.IntegrationEvent { Id = Guid.NewGuid() };

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
