using Xunit;
using Moq;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

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
            var eventLogServiceMock = new Mock<eShop.Ordering.API.Application.IntegrationEvents.IIntegrationEventLogService>();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<eShop.Ordering.API.Application.IntegrationEvents.OrderingIntegrationEventService>>();
            var service = new eShop.Ordering.API.Application.IntegrationEvents.OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

            var pendingLogEvents = new[] { new eShop.Ordering.API.Application.IntegrationEvents.IntegrationEventLog { EventId = Guid.NewGuid(), IntegrationEvent = new UserRegistrationIntegrationEvent(Guid.NewGuid(), "test", "test") } };
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
            var eventBusMock = new Mock<eShop.Ordering.API.Application.IntegrationEvents.IEventBus>();
            var orderingContextMock = new Mock<eShop.Ordering.API.Infrastructure.OrderingContext>();
            var eventLogServiceMock = new Mock<eShop.Ordering.API.Application.IntegrationEvents.IIntegrationEventLogService>();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<eShop.Ordering.API.Application.IntegrationEvents.OrderingIntegrationEventService>>();
            var service = new eShop.Ordering.API.Application.IntegrationEvents.OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

            var integrationEvent = new UserRegistrationIntegrationEvent(Guid.NewGuid(), "test", "test");
            eventLogServiceMock.Setup(es => es.SaveEventAsync(It.IsAny<eShop.Ordering.API.Application.IntegrationEvents.IntegrationEvent>(), It.IsAny<object>())).ReturnsAsync(Task.CompletedTask);

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
