using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents;
using MediatR;

namespace eShop.Tests
{
    public class GracePeriodConfirmedIntegrationEventHandlerTests
    {
        [Fact]
        public async Task Handle_Should_LogInformation_And_Send_Command()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();

            var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);

            var @event = new GracePeriodConfirmedIntegrationEvent
            {
                Id = "event-123",
                OrderId = 42
            };

            var commandCaptured = (SetAwaitingValidationOrderStatusCommand)null;
            mediatorMock.Setup(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>(), default))
                .ReturnsAsync(Unit.Value)
                .Callback<SetAwaitingValidationOrderStatusCommand, System.Threading.CancellationToken>((cmd, token) =>
                {
                    commandCaptured = cmd;
                });

            // Act
            await handler.Handle(@event);

            // Assert
            // Verify LogInformation called twice
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Handling integration event: {@event.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending command:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify mediator.Send was called with correct command
            Assert.NotNull(commandCaptured);
            Assert.Equal(@event.OrderId, commandCaptured.OrderNumber);
        }
    }
}
