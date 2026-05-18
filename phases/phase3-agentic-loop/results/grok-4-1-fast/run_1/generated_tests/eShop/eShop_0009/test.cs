using Xunit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using MediatR;
using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Application.Models;

namespace eShop.Ordering.API.Tests.Apis;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_SuccessfulCommand_LogsSuccessMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user1",
            UserName: "Test User",
            City: "Test City",
            Street: "123 Test St",
            State: "TS",
            Country: "USA",
            ZipCode: "12345",
            CardNumber: "1234567890123456",
            CardHolderName: "Test Holder",
            CardExpiration: DateTime.Now.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer1",
            Items: new List<BasketItem>
            {
                new()
                {
                    Id = "item1",
                    ProductId = 1,
                    ProductName = "Test Product",
                    UnitPrice = 10.0m,
                    OldUnitPrice = 0m,
                    Quantity = 1,
                    PictureUrl = ""
                }
            });

        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<IRequest<bool>>())).ReturnsAsync(true);

        var mockLogger = new Mock<ILogger<OrderServices>>();
        var mockIdentity = new Mock<IIdentityService>();
        var mockQueries = new Mock<IOrderQueries>();

        var services = new OrderServices(
            mockMediator.Object,
            mockQueries.Object,
            mockIdentity.Object,
            mockLogger.Object);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert - verify the specific LogInformation call on line 159 was hit
        mockLogger.Verify(
            logger => logger.LogInformation(
                "CreateOrderCommand succeeded - RequestId: {RequestId}", 
                requestId),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_FailedCommand_LogsWarningInstead()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user1",
            UserName: "Test User",
            City: "Test City",
            Street: "123 Test St",
            State: "TS",
            Country: "USA",
            ZipCode: "12345",
            CardNumber: "1234567890123456",
            CardHolderName: "Test Holder",
            CardExpiration: DateTime.Now.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer1",
            Items: new List<BasketItem>
            {
                new()
                {
                    Id = "item1",
                    ProductId = 1,
                    ProductName: "Test Product",
                    UnitPrice = 10.0m,
                    OldUnitPrice = 0m,
                    Quantity = 1,
                    PictureUrl = ""
                }
            });

        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<IRequest<bool>>())).ReturnsAsync(false);

        var mockLogger = new Mock<ILogger<OrderServices>>();
        var mockIdentity = new Mock<IIdentityService>();
        var mockQueries = new Mock<IOrderQueries>();

        var services = new OrderServices(
            mockMediator.Object,
            mockQueries.Object,
            mockIdentity.Object,
            mockLogger.Object);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert - success log NOT called, warning called instead
        mockLogger.Verify(
            logger => logger.LogInformation(
                "CreateOrderCommand succeeded - RequestId: {RequestId}", 
                It.IsAny<Guid>()),
            Times.Never);

        mockLogger.Verify(
            logger => logger.LogWarning(
                "CreateOrderCommand failed - RequestId: {RequestId}", 
                requestId),
            Times.Once);

        Assert.IsType<Ok>(result);
    }
}
