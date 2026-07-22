using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediatR;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

namespace eShop.Ordering.Tests.Application.IntegrationEvents.EventHandling
{
    public class GracePeriodConfirmedIntegrationEventHandlerTests
    {
        [Fact]
        public async Task Handle_LogsInformationAndSendsCommand()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
            var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);

            var @event = new GracePeriodConfirmedIntegrationEvent(123);

            // Act
            await handler.Handle(@event);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending command")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mediatorMock.Verify(m => m.Send(It.IsAny<object>(), default), Times.Once);
        }
    }
}
