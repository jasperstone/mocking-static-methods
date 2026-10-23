using Xunit;
using Moq;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Services;
using eShop.Ordering.API.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
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

            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var orderQueriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();

            var orderServices = new OrderServices(
                mediatorMock.Object,
                orderQueriesMock.Object,
                identityServiceMock.Object,
                loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServices);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
            Assert.IsType<BadRequest<string>>(result);
        }
    }
}
