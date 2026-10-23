using Xunit;
using Moq;
using eShop.Ordering.API.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Infrastructure.Repositories;

namespace eShop.Ordering.API.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation()
        {
            // Arrange
            var eventBusMock = new Mock<MediatR.IMediator>();
            var orderingContextMock = new Mock<OrderingContext>();
            var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
            integrationEventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>())).ReturnsAsync(new[] { new IntegrationEventLog() { EventId = Guid.NewGuid(), IntegrationEvent = new IntegrationEvent() } });
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, integrationEventLogServiceMock.Object, loggerMock.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
