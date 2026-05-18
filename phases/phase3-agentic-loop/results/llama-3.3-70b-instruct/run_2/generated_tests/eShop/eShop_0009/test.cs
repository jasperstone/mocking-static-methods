using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Services;
using eShop.Ordering.API.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_ValidRequest_ReturnsOkResult()
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
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(true);

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var orderServicesMock = new Mock<IOrderServices>();
            orderServicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            orderServicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServicesMock.Object);

            // Assert
            Assert.IsType<Ok>(result);
            loggerMock.Verify(l => l.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestId), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_ReturnsBadRequestResult()
        {
            // Arrange
            var requestId = Guid.Empty;
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
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var orderServicesMock = new Mock<IOrderServices>();
            orderServicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            orderServicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServicesMock.Object);

            // Assert
            Assert.IsType<BadRequest<string>>(result);
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_CreateOrderCommandFailed_ReturnsOkResult()
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
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(false);

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var orderServicesMock = new Mock<IOrderServices>();
            orderServicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            orderServicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServicesMock.Object);

            // Assert
            Assert.IsType<Ok>(result);
            loggerMock.Verify(l => l.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId), Times.Once);
        }
    }
}
