using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static eShop.Ordering.API.Apis.OrdersApi;

namespace eShop.Ordering.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_WithEmptyRequestId_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
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
                ZipCode: "12345",
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
            loggerMock.Verify(
                x => x.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request),
                Times.Once);
        }
    }
}
