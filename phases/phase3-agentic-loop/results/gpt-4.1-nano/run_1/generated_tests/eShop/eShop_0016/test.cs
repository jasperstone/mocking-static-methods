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
            var mockLogger = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
            var mockMediator = new Mock<IMediator>();
            var handler = new GracePeriodConfirmedIntegrationEventHandler(mockMediator.Object, mockLogger.Object);

            var @event = new GracePeriodConfirmedIntegrationEvent
            {
                Id = "event-id-123",
                OrderId = 42
            };

            var command = new SetAwaitingValidationOrderStatusCommand(@event.OrderId);
            mockMediator.Setup(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>()))
                        .ReturnsAsync(Unit.Value);

            // Act
            await handler.Handle(@event);

            // Assert
            // Verify LogInformation was called with the expected message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Handling integration event: {@event.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that mediator.Send was called with a command with the correct OrderId
            mockMediator.Verify(m => m.Send(It.Is<SetAwaitingValidationOrderStatusCommand>(cmd => cmd.OrderNumber == @event.OrderId)), Times.Once);
        }
    }
}
