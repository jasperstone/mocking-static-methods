using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly Mock<IIntegrationEventLogService> _eventLogServiceMock;
        private readonly Mock<ILogger<OrderingIntegrationEventService>> _loggerMock;
        private readonly Mock<OrderingContext> _orderingContextMock;
        private readonly OrderingIntegrationEventService _service;

        public OrderingIntegrationEventServiceTests()
        {
            _eventBusMock = new Mock<IEventBus>();
            _eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            _loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            _orderingContextMock = new Mock<OrderingContext>();

            _service = new OrderingIntegrationEventService(
                _eventBusMock.Object,
                _orderingContextMock.Object,
                _eventLogServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformationAndCallsSave()
        {
            // Arrange
            var evt = new IntegrationEvent { Id = Guid.NewGuid() };
            var transactionMock = new Mock<IDbContextTransaction>();
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns(transactionMock.Object);

            // Act
            await _service.AddAndSaveEventAsync(evt);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Enqueuing")),
                    evt.Id,
                    It.IsAny<IntegrationEvent>()),
                Times.Once);

            _eventLogServiceMock.Verify(s => s.SaveEventAsync(evt, transactionMock.Object), Times.Once);
        }
    }
}
