using Xunit;
using Moq;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
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
            var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequestId_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
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
            var requestId = Guid.NewGuid();

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateOrderAsync_CreateOrderCommandFailed_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
            servicesMock.Setup(s => s.Mediator.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>())).ReturnsAsync(false);
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
            var requestId = Guid.NewGuid();

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId), Times.Once);
        }
    }
}
