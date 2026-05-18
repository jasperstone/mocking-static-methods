using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var servicesMock = new Mock<OrdersApi.OrderServices>();
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
            await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequestId_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var servicesMock = new Mock<OrdersApi.OrderServices>();
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
            await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
