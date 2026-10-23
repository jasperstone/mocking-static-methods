using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static eShop.Ordering.API.Apis.OrdersApi;

namespace OrderingApiTests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_RequestIdEmpty_LogsWarning()
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
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Invalid IntegrationEvent - RequestId is missing")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }

    // Dummy classes to compile the test
    public class BasketItem { }
    public interface IMediator
    {
        Task<T> Send<T>(object command);
    }
    public interface IIdentityService
    {
        string GetUserIdentity();
    }
    public interface IOrderQueries
    {
        Task<IEnumerable<object>> GetCardTypesAsync();
        Task<object> GetOrderAsync(int orderId);
        Task<IEnumerable<object>> GetOrdersFromUserAsync(string userId);
    }
    public class OrderServices
    {
        public ILogger<OrdersApi> Logger { get; set; }
        public IMediator Mediator { get; set; }
        public IIdentityService IdentityService { get; set; }
        public IOrderQueries Queries { get; set; }
    }
}
