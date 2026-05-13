using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Apis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_LogsWarning_WhenRequestIdIsMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var services = new OrderServices
            {
                Logger = loggerMock.Object,
                Mediator = mediatorMock.Object
            };

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
                Items = new List<BasketItem>()
            );

            // Act
            var result = await new OrdersApi().CreateOrderAsync(Guid.Empty, request, services);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Invalid IntegrationEvent - RequestId is missing")),
                    It.IsAny<CreateOrderRequest>()), Times.Once);

            Assert.IsType<TypedResults.BadRequest<string>>(result);
            Assert.Equal("RequestId is missing.", ((TypedResults.BadRequest<string>)result).Value);
        }
    }
}
