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
    public async Task CreateOrderAsync_LogsWarning_WhenRequestIdIsEmpty()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrdersApi>>();
        var servicesMock = new Mock<OrderServices>();
        servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);

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
            CardExpiration = DateTime.Now,
            CardSecurityNumber = "123",
            CardTypeId = 1,
            Buyer = "Test Buyer",
            Items = new List<BasketItem>());

        var api = new OrdersApi();

        // Act
        var result = await api.CreateOrderAsync(Guid.Empty, request, servicesMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(
                It.Is<string>(s => s.Contains("Invalid IntegrationEvent - RequestId is missing")),
                It.IsAny<CreateOrderRequest>()),
            Times.Once);

        Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", result.Value);
    }
}
