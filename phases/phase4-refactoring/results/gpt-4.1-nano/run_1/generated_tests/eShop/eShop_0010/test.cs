using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static eShop.Ordering.API.Apis.OrdersApi;

namespace eShop.Ordering.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_RequestIdEmpty_ShouldLogWarningAndReturnBadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var identityServiceMock = new Mock<IIdentityService>();
            var queryMock = new Mock<IOrderQueries>();
            var services = new OrderServices
            {
                Logger = loggerMock.Object,
                Mediator = mediatorMock.Object,
                IdentityService = identityServiceMock.Object,
                Queries = queryMock.Object
            };

            var request = new CreateOrderRequest(
                UserId: "user1",
                UserName: "User One",
                City: "City",
                Street: "Street",
                State: "State",
                Country: "Country",
                ZipCode: "Zip",
                CardNumber: "1234567890123456",
                CardHolderName: "Holder",
                CardExpiration: DateTime.UtcNow.AddYears(1),
                CardSecurityNumber: "123",
                CardTypeId: 1,
                Buyer: "Buyer",
                Items: new List<BasketItem>());

            // Act
            var result = await CreateOrderAsync(Guid.Empty, request, services);

            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("RequestId is missing.", badRequestResult.Value);
            loggerMock.Verify(
                x => x.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request),
                Times.Once);
        }
    }
}
