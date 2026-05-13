using Xunit;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling.Tests
{
    public class GracePeriodConfirmedIntegrationEventHandlerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>> _loggerMock;
        private readonly GracePeriodConfirmedIntegrationEventHandler _handler;

        public GracePeriodConfirmedIntegrationEventHandlerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
            _handler = new GracePeriodConfirmedIntegrationEventHandler(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldLogInformation_WhenEventIsHandled()
        {
            // Arrange
            var integrationEvent = new GracePeriodConfirmedIntegrationEvent(1);

            // Act
            await _handler.Handle(integrationEvent);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(1));
        }
    }
}
