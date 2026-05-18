using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Threading;
using System.Linq;

namespace OrderingApiTests
{
    public class OrdersApiTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly OrderServices _services;

        public OrdersApiTests()
        {
            _loggerMock = new Mock<ILogger>();
            _mediatorMock = new Mock<IMediator>();
            _services = new OrderServices
            {
                Logger = _loggerMock.Object,
                Mediator = _mediatorMock.Object,
                Queries = null,
                IdentityService = null
            };
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
                CardExpiration: DateTime.Now.AddYears(1),
                CardSecurityNumber: "123",
                CardTypeId: 1,
                Buyer: "Buyer",
                Items: new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, _services);

            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("RequestId is missing.", badRequestResult.Value);
            _loggerMock.Verify(
                x => x.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request),
                Times.Once);
        }
    }
}
