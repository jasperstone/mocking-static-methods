using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Services;
using eShop.Ordering.API.Infrastructure.Services;
using MediatR;
using System;
using System.Threading.Tasks;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_LogsWarning_WhenCreateOrderCommandFails()
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

            var loggerMock = new Mock<ILogger<OrderServices>>();
            var orderServices = new OrderServices(mediatorMock.Object, Mock.Of<IOrderQueries>(), Mock.Of<IIdentityService>(), loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServices);

            // Assert
            loggerMock.Verify(l => l.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId), Times.Once);
        }
    }
}
