using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using System.Collections.Generic;
using MediatR;

namespace Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_LogsWarningWhenCreateOrderCommandFails()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                "UserId",
                "UserName",
                "City",
                "Street",
                "State",
                "Country",
                "ZipCode",
                "CardNumber",
                "CardHolderName",
                DateTime.Now,
                "CardSecurityNumber",
                1,
                "Buyer",
                new List<BasketItem>());

            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
                .ReturnsAsync(false);

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId), Times.Once);
        }
    }
}
