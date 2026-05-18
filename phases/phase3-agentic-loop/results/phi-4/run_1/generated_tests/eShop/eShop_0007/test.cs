using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using eShop.Ordering.API.Models; // Ensure this using directive is included

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_LogsWarning_WhenRequestIdIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);

            var request = new CreateOrderRequest(
                UserId = "user123",
                UserName = "Test User",
                City = "Test City",
                Street = "Test Street",
                State = "Test State",
                Country = "Test Country",
                ZipCode = "12345",
                CardNumber = "1234567890123456",
                CardHolderName = "Test Holder",
                CardExpiration = DateTime.Now,
                CardSecurityNumber = "123",
                CardTypeId = 1,
                Buyer = "Test Buyer",
                Items = new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s.Contains("Invalid IntegrationEvent - RequestId is missing")),
                    It.IsAny<CreateOrderRequest>()), Times.Once);

            Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("RequestId is missing.", result.Value);
        }
    }
}
