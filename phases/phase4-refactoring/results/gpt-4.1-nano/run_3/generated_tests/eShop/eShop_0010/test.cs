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
        public async Task CreateOrderAsync_RequestIdEmpty_ShouldLogWarningAndReturnBadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var services = new OrderServices
            {
                Logger = loggerMock.Object,
                Mediator = mediatorMock.Object,
                IdentityService = new Mock<IIdentityService>().Object,
                Queries = new Mock<IOrderQueries>().Object
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

        [Fact]
        public async Task CreateOrderAsync_ValidRequest_ShouldLogInformationAndReturnOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var requestId = Guid.NewGuid();
            var services = new OrderServices
            {
                Logger = loggerMock.Object,
                Mediator = mediatorMock.Object,
                IdentityService = new Mock<IIdentityService>().Object,
                Queries = new Mock<IOrderQueries>().Object
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

            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await CreateOrderAsync(requestId, request, services);

            // Assert
            var okResult = Assert.IsType<Results<Ok, BadRequest<string>>>(result);
            Assert.IsType<OkResult>(okResult);
            loggerMock.Verify(
                x => x.LogInformation("Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})", 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()), 
                Times.Once);
            loggerMock.Verify(
                x => x.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestId),
                Times.Once);
        }
    }
}
