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

            // Setup mediator to complete successfully
            mockMediator.Setup(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>(), default))
                        .ReturnsAsync(Unit.Value);

            // Act
            await handler.Handle(@event);

            // Assert
            // Verify that LogInformation was called at least once with the expected message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify that mediator.Send was called with the correct command
            mockMediator.Verify(m => m.Send(It.Is<SetAwaitingValidationOrderStatusCommand>(cmd => cmd.OrderNumber == @event.OrderId), default), Times.Once);
        }
    }
}
