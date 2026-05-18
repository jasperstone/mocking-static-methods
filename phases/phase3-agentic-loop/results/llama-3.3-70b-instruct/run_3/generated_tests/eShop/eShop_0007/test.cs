using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using Ordering.API.Application.Models;
using Ordering.API.Application.Services;
using Ordering.API.Apis;

namespace Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var mediatorMock = new Mock<IMediator>();
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

            var orderServices = new OrderServices(mediatorMock.Object, null, null, loggerMock.Object);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, orderServices);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }
    }
}
