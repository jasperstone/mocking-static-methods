using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System.Threading.Tasks;

namespace eShop.Ordering.API.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<eShop.Ordering.Domain.Core.Interfaces.IEventBus>();
            var orderingContextMock = new Mock<eShop.Ordering.Infrastructure.Data.OrderingContext>();
            var eventLogServiceMock = new Mock<eShop.Ordering.Infrastructure.Services.IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<eShop.Ordering.Domain.Core.Interfaces.IEventBus>();
            var orderingContextMock = new Mock<eShop.Ordering.Infrastructure.Data.OrderingContext>();
            var eventLogServiceMock = new Mock<eShop.Ordering.Infrastructure.Services.IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);
            var integrationEvent = new eShop.Ordering.Domain.Events.IntegrationEvent { Id = Guid.NewGuid() };

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
