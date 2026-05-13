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

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var orderServicesMock = new Mock<OrderServices>();
            orderServicesMock.Setup(os => os.Logger).Returns(loggerMock.Object);
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
            Assert.IsType<BadRequest<string>>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequestId_LogsInformationAndReturnsOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var orderServicesMock = new Mock<OrderServices>();
            orderServicesMock.Setup(os => os.Logger).Returns(loggerMock.Object);
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
            loggerMock.Verify(l => l.LogInformation("Sending command: {CommandName} - {IdProperty}: {CommandId}", request.GetGenericTypeName(), nameof(request.UserId), request.UserId), Times.Once);
            Assert.IsType<Ok>(result);
        }
    }
}
