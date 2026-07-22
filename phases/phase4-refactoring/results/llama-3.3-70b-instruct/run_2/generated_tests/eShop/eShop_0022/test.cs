using Xunit;
using Moq;
using eShop.Ordering.API.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Infrastructure;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.EventBus.Abstractions;

namespace eShop.Ordering.API.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<IEventBus>();
            var orderingContextMock = new Mock<eShop.Ordering.Infrastructure.OrderingContext>();
            var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
            integrationEventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>())).ReturnsAsync(new[] { new IntegrationEventLog { EventId = Guid.NewGuid(), IntegrationEvent = new UserRegisteredIntegrationEvent(Guid.NewGuid(), "test", "test") } });
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, integrationEventLogServiceMock.Object, loggerMock.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
