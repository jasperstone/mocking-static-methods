using Xunit;
using Moq;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Services;
using eShop.Ordering.API.Infrastructure;
using eShop.Ordering.API.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
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
            var request = new CreateOrderRequest("UserId", "UserName", "City", "Street", "State", "Country", "ZipCode", "CardNumber", "CardHolderName", DateTime.Now, "CardSecurityNumber", 1, "Buyer", new List<BasketItem>());
            var orderServices = new Mock<IOrderServices>();
            var mediator = new Mock<IMediator>();
            var logger = new Mock<ILogger>();

            orderServices.Setup(s => s.Logger).Returns(logger.Object);
            mediator.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>())).ReturnsAsync(true);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServices.Object);

            // Assert
            Assert.IsType<Ok>(result);
            logger.Verify(l => l.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestId), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_InvalidRequest_ReturnsBadRequestResult()
        {
            // Arrange
            var requestId = Guid.Empty;
            var request = new CreateOrderRequest("UserId", "UserName", "City", "Street", "State", "Country", "ZipCode", "CardNumber", "CardHolderName", DateTime.Now, "CardSecurityNumber", 1, "Buyer", new List<BasketItem>());
            var orderServices = new Mock<IOrderServices>();
            var mediator = new Mock<IMediator>();
            var logger = new Mock<ILogger>();

            orderServices.Setup(s => s.Logger).Returns(logger.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServices.Object);

            // Assert
            Assert.IsType<BadRequest<string>>(result);
            logger.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }
    }
}
