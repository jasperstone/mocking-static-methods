using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

namespace eShop.Tests
{
    public class GracePeriodConfirmedIntegrationEventHandlerTests
    {
        [Fact]
        public async Task Handle_Should_LogInformation_And_SendCommand()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();

            var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);

            var @event = new GracePeriodConfirmedIntegrationEvent(orderId: 123);
            var command = new SetAwaitingValidationOrderStatusCommand(@event.OrderId);

            mediatorMock.Setup(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>(), default))
                        .ReturnsAsync(Unit.Value);

            // Act
            await handler.Handle(@event);

            // Assert
            // Verify LogInformation called with message containing the event Id and event
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Handling integration event: {@event.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify mediator.Send called with a command with the correct OrderId
            mediatorMock.Verify(m => m.Send(It.Is<SetAwaitingValidationOrderStatusCommand>(c => c.OrderNumber == @event.OrderId), default), Times.Once);
        }
    }
}
