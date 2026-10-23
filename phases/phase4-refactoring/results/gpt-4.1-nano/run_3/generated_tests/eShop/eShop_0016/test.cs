using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
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

            var commandCaptured = (SetAwaitingValidationOrderStatusCommand)null;
            mediatorMock.Setup(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>()))
                .Callback<object>(cmd => commandCaptured = (SetAwaitingValidationOrderStatusCommand)cmd)
                .ReturnsAsync(true);

            // Act
            await handler.Handle(@event);

            // Assert
            // Verify LogInformation was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Handling integration event: {@event.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify mediator.Send was called with the correct command
            Assert.NotNull(commandCaptured);
            Assert.Equal(@event.OrderId, commandCaptured.OrderNumber);
        }
    }
}
