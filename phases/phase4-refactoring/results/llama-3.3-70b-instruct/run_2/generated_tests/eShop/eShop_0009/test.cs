using Xunit;
using Moq;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Services;
using eShop.Ordering.API.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var request = new CreateOrderRequest
            {
                UserId = "user1",
                UserName = "User 1",
                City = "City 1",
                Street = "Street 1",
                State = "State 1",
                Country = "Country 1",
                ZipCode = "ZipCode 1",
                CardNumber = "CardNumber 1",
                CardHolderName = "CardHolderName 1",
                CardExpiration = DateTime.Now,
                CardSecurityNumber = "CardSecurityNumber 1",
                CardTypeId = 1,
                Buyer = "Buyer 1",
                Items = new List<BasketItem>
                {
                    new BasketItem { ProductId = 1, ProductName = "Product 1", Quantity = 1, UnitPrice = 10.99m }
                }
            };

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var services = new OrderServices(mediatorMock.Object, new Mock<IOrderQueries>().Object, loggerMock.Object, new Mock<IIdentityService>().Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_ReturnsBadRequestResult()
        {
            // Arrange
            var requestId = Guid.Empty;
            var request = new CreateOrderRequest
            {
                UserId = "user1",
                UserName = "User 1",
                City = "City 1",
                Street = "Street 1",
                State = "State 1",
                Country = "Country 1",
                ZipCode = "ZipCode 1",
                CardNumber = "CardNumber 1",
                CardHolderName = "CardHolderName 1",
                CardExpiration = DateTime.Now,
                CardSecurityNumber = "CardSecurityNumber 1",
                CardTypeId = 1,
                Buyer = "Buyer 1",
                Items = new List<BasketItem>
                {
                    new BasketItem { ProductId = 1, ProductName = "Product 1", Quantity = 1, UnitPrice = 10.99m }
                }
            };

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var services = new OrderServices(mediatorMock.Object, new Mock<IOrderQueries>().Object, loggerMock.Object, new Mock<IIdentityService>().Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_CreateOrderCommandSucceeded_LogsInformation()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest
            {
                UserId = "user1",
                UserName = "User 1",
                City = "City 1",
                Street = "Street 1",
                State = "State 1",
                Country = "Country 1",
                ZipCode = "ZipCode 1",
                CardNumber = "CardNumber 1",
                CardHolderName = "CardHolderName 1",
                CardExpiration = DateTime.Now,
                CardSecurityNumber = "CardSecurityNumber 1",
                CardTypeId = 1,
                Buyer = "Buyer 1",
                Items = new List<BasketItem>
                {
                    new BasketItem { ProductId = 1, ProductName = "Product 1", Quantity = 1, UnitPrice = 10.99m }
                }
            };

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>())).ReturnsAsync(true);
            var services = new OrderServices(mediatorMock.Object, new Mock<IOrderQueries>().Object, loggerMock.Object, new Mock<IIdentityService>().Object);

            // Act
            await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            loggerMock.Verify(l => l.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestId), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_CreateOrderCommandFailed_LogsWarning()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest
            {
                UserId = "user1",
                UserName = "User 1",
                City = "City 1",
                Street = "Street 1",
                State = "State 1",
                Country = "Country 1",
                ZipCode = "ZipCode 1",
                CardNumber = "CardNumber 1",
                CardHolderName = "CardHolderName 1",
                CardExpiration = DateTime.Now,
                CardSecurityNumber = "CardSecurityNumber 1",
                CardTypeId = 1,
                Buyer = "Buyer 1",
                Items = new List<BasketItem>
                {
                    new BasketItem { ProductId = 1, ProductName = "Product 1", Quantity = 1, UnitPrice = 10.99m }
                }
            };

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>())).ReturnsAsync(false);
            var services = new OrderServices(mediatorMock.Object, new Mock<IOrderQueries>().Object, loggerMock.Object, new Mock<IIdentityService>().Object);

            // Act
            await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            loggerMock.Verify(l => l.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId), Times.Once);
        }
    }
}
