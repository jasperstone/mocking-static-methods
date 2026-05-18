using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Tests
{
    public class GracePeriodConfirmedIntegrationEventHandlerTests
    {
        [Fact]
        public async Task Handle_Should_LogInformationAndSendCommand()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
            var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);

            var @event = new GracePeriodConfirmedIntegrationEvent
            {
                Id = "event-id-123",
                OrderId = 42
            };

            var command = new SetAwaitingValidationOrderStatusCommand(@event.OrderId);
            mediatorMock.Setup(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>()))
                        .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(@event);

            // Assert
            // Verify LogInformation was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that mediator.Send was called with a command with the correct OrderId
            mediatorMock.Verify(m => m.Send(It.Is<SetAwaitingValidationOrderStatusCommand>(c => c.OrderNumber == @event.OrderId)), Times.Once);
        }
    }
}
