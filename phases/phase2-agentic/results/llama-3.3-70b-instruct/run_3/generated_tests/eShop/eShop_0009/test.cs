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
        public async Task CreateOrderAsync_ValidRequest_LoggerInformationCalled()
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

            var loggerMock = new Mock<ILogger<OrderServices>>();
            var mediatorMock = new Mock<IMediator>();
            var orderServices = new OrderServices(loggerMock.Object, mediatorMock.Object, null);

            // Act
            await OrdersApi.CreateOrderAsync(requestId, request, orderServices);

            // Assert
            loggerMock.Verify(l => l.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestId), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_InvalidRequest_LoggerWarningCalled()
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

            var loggerMock = new Mock<ILogger<OrderServices>>();
            var mediatorMock = new Mock<IMediator>();
            var orderServices = new OrderServices(loggerMock.Object, mediatorMock.Object, null);

            // Act
            await OrdersApi.CreateOrderAsync(requestId, request, orderServices);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }
    }
}
