using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrderingApiTests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
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
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
            Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("RequestId is missing.", ((BadRequest<string>)result).Value);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequestId_LogsInformationAndReturnsOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
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
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.IsType<Results<Ok, BadRequest<string>>>(result);
        }
    }
}
