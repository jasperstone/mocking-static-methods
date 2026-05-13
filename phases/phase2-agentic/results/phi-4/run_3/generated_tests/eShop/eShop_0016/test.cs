using System.Threading.Tasks;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling.Tests
{
    public class GracePeriodConfirmedIntegrationEventHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldLogInformationAndSendCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
            var mediatorMock = new Mock<IMediator>();
            var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);

            var @event = new GracePeriodConfirmedIntegrationEvent(orderId: 1);
            var command = new SetAwaitingValidationOrderStatusCommand(@event.OrderId);

            // Act
            await handler.Handle(@event);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})",
                    @event.Id,
                    It.IsAny<object>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation(
                    "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    command.GetType().Name,
                    nameof(command.OrderNumber),
                    command.OrderNumber,
                    It.IsAny<object>()),
                Times.Once);

            mediatorMock.Verify(
                x => x.Send(command),
                Times.Once);
        }
    }
}
