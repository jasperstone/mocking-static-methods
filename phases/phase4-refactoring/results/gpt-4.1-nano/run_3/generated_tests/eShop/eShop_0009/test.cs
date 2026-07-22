using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace eShop.Ordering.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_LogsInformationAndReturnsOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrdersApi>>();
            var mediatorMock = new Mock<IMediator>();
            var identityServiceMock = new Mock<IIdentityService>();
            var queryMock = new Mock<IOrderQueries>();
            var services = new OrderServices(
                mediatorMock.Object,
                queryMock.Object,
                identityServiceMock.Object,
                loggerMock.Object
            );

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
                Items: new List<BasketItem> { new BasketItem() }
            );

            var requestId = Guid.NewGuid();

            // Setup mediator to return true
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
                .ReturnsAsync(true);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TypedResults.OkResult>(result);
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Sending command")), 
                It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }

    // Dummy interfaces and classes to make the test compile
    public interface IMediator
    {
        Task<T> Send<T>(object request);
    }

    public interface IIdentityService
    {
        string GetUserIdentity();
    }

    public interface IOrderQueries
    {
        // methods omitted
    }

    public class BasketItem { }

    public class OrderServices
    {
        public IMediator Mediator { get; }
        public IOrderQueries Queries { get; }
        public IIdentityService IdentityService { get; }
        public ILogger<OrderServices> Logger { get; }

        public OrderServices(IMediator mediator, IOrderQueries queries, IIdentityService identityService, ILogger<OrderServices> logger)
        {
            Mediator = mediator;
            Queries = queries;
            IdentityService = identityService;
            Logger = logger;
        }
    }
}
