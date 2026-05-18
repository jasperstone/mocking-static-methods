using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using MediatR;
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
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
                .ReturnsAsync(true);

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var orderQueriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var services = new OrderServices(mediatorMock.Object, orderQueriesMock.Object, identityServiceMock.Object, loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

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
            var orderQueriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var services = new OrderServices(mediatorMock.Object, orderQueriesMock.Object, identityServiceMock.Object, loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            Assert.IsType<BadRequest<string>>(result);
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }
    }
}
