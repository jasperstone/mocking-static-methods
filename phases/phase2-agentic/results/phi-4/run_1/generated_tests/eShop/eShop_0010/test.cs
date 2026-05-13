using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Apis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_LogsWarning_WhenCreateOrderCommandFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrdersApi>>();
        var mediatorMock = new Mock<IMediator>();

        var services = new OrderServices
        {
            Logger = loggerMock.Object,
            Mediator = mediatorMock.Object
        };

        var requestId = Guid.NewGuid();
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
            CardExpiration = DateTime.UtcNow.AddYears(1),
            CardSecurityNumber = "123",
            CardTypeId = 1,
            Buyer = "Test Buyer",
            Items = new List<BasketItem> { new BasketItem { ProductId = "prod1", Quantity = 1 } }
        );

        mediatorMock
            .Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
            .ReturnsAsync(false); // Simulate command failure

        var ordersApi = new OrdersApi();

        // Act
        var result = await ordersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(
                It.Is<string>(s => s.Contains("CreateOrderCommand failed")),
                It.Is<Guid>(id => id == requestId)),
            Times.Once);
    }
}
