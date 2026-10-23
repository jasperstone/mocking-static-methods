using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using static eShop.Ordering.API.Apis.OrdersApi;

namespace OrderingApiTests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_LogsInformationCall()
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

            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                UserId: "user123",
                UserName: "John Doe",
                City: "CityX",
                Street: "StreetY",
                State: "StateZ",
                Country: "CountryA",
                ZipCode: "12345",
                CardNumber: "1234567890123456",
                CardHolderName: "John Doe",
                CardExpiration: DateTime.UtcNow.AddYears(1),
                CardSecurityNumber: "123",
                CardTypeId: 1,
                Buyer: "BuyerX",
                Items: new List<BasketItem>());

            // Setup mediator to return true
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await CreateOrderAsync(requestId, request, services);

            // Assert
            Assert.IsType<Results<Ok, BadRequest<string>>>(result);
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Sending command:")),
                    It.IsAny<object[]>()
                ),
                Times.AtLeastOnce);
        }
    }

    // Dummy classes to compile the test
    public class BasketItem { }
    public interface IIdentityService { string GetUserIdentity(); }
    public interface IOrderQueries { Task<IEnumerable<object>> GetCardTypesAsync(); }
    public interface IMediator { Task<bool> Send(object command, System.Threading.CancellationToken token = default); }
    public class OrderServices
    {
        public ILogger<OrdersApi> Logger { get; set; }
        public IMediator Mediator { get; set; }
        public IIdentityService IdentityService { get; set; }
        public IOrderQueries Queries { get; set; }
    }
}
