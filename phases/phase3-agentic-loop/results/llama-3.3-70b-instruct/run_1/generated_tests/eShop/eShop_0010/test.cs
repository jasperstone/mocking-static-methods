using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var orderQueriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var orderServicesMock = new Mock<OrderServices>();
            orderServicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
            orderServicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            orderServicesMock.Setup(s => s.Queries).Returns(orderQueriesMock.Object);
            orderServicesMock.Setup(s => s.IdentityService).Returns(identityServiceMock.Object);
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

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequestId_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var orderQueriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var orderServicesMock = new Mock<OrderServices>();
            orderServicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
            orderServicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            orderServicesMock.Setup(s => s.Queries).Returns(orderQueriesMock.Object);
            orderServicesMock.Setup(s => s.IdentityService).Returns(identityServiceMock.Object);
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

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_CreateOrderCommandFailed_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var orderQueriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var orderServicesMock = new Mock<OrderServices>();
            orderServicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
            orderServicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            orderServicesMock.Setup(s => s.Queries).Returns(orderQueriesMock.Object);
            orderServicesMock.Setup(s => s.IdentityService).Returns(identityServiceMock.Object);
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>())).ReturnsAsync(false);
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

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId), Times.Once);
        }
    }
}
