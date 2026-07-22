using Xunit;
using Moq;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eShop.Ordering.API.Tests
{
    public class GracePeriodConfirmedIntegrationEventHandlerTests
    {
        [Fact]
        public async Task Handle_LogsInformation_WhenHandlingIntegrationEvent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
            var mediatorMock = new Mock<IMediator>();
            var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);
            var @event = new GracePeriodConfirmedIntegrationEvent(1);

            // Act
            await handler.Handle(@event);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_SendsCommand_WhenHandlingIntegrationEvent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
            var mediatorMock = new Mock<IMediator>();
            var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);
            var @event = new GracePeriodConfirmedIntegrationEvent(1);

            // Act
            await handler.Handle(@event);

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>()), Times.Once);
        }
    }
}
