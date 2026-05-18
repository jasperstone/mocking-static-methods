using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using src.Ordering.API.Apis;
using Microsoft.AspNetCore.Http.HttpResults;
using static src.Ordering.API.Apis.OrdersApi;

namespace OrderingApiTests
{
    public class OrdersApiTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IOrderQueries> _queriesMock;
        private readonly Mock<IIdentityService> _identityServiceMock;

        public OrdersApiTests()
        {
            _loggerMock = new Mock<ILogger>();
            _mediatorMock = new Mock<IMediator>();
            _queriesMock = new Mock<IOrderQueries>();
            _identityServiceMock = new Mock<IIdentityService>();
        }

        [Fact]
        public async Task CreateOrderAsync_Should_Log_Warning_When_RequestId_Is_Empty()
        {
            // Arrange
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

            var services = new OrderServices
            {
                Logger = _loggerMock.Object,
                Mediator = _mediatorMock.Object,
                Queries = _queriesMock.Object,
                IdentityService = _identityServiceMock.Object
            };

            // Act
            var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, services);

            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("RequestId is missing.", badRequestResult.Value);
            _loggerMock.Verify(
                x => x.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request),
                Times.Once);
        }
    }
}
