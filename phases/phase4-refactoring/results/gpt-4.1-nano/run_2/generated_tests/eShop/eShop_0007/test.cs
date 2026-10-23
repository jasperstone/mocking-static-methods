using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static eShop.Ordering.API.Apis.OrdersApi;

namespace eShop.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_WithEmptyRequestId_LogsWarningAndReturnsBadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<Microsoft.Extensions.Mediator.IMediator>();
            var queriesMock = new Mock<eShop.Ordering.API.Application.Queries.IOrderQueries>();
            var identityServiceMock = new Mock<eShop.Ordering.API.Application.Queries.IIdentityService>();

            var services = new OrderServices(
                mediatorMock.Object,
                queriesMock.Object,
                identityServiceMock.Object,
                loggerMock.Object
            );

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
            var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, services);

            // Assert
            // Verify that LogWarning was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid IntegrationEvent - RequestId is missing")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that the result is BadRequest
            Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>>(result);
            var badRequestResult = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
            Assert.Equal("RequestId is missing.", badRequestResult?.Value);
        }
    }

    // Dummy BasketItem class for test
    public class BasketItem { }
}
