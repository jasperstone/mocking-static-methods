using Xunit;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling.Tests
{
    public class GracePeriodConfirmedIntegrationEventHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldLogInformationAndSendCommand()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();

            var eventHandler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);
            var integrationEvent = new GracePeriodConfirmedIntegrationEvent(1);

            // Act
            await eventHandler.Handle(integrationEvent);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})",
                    It.IsAny<object>(),
                    It.IsAny<object>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation(
                    "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>()),
                Times.Once);

            mediatorMock.Verify(
                x => x.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
