using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using eShop.Ordering.API.Application.Models;
using MediatR;

namespace eShop.Ordering.API.Tests.Apis;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenRequestIdIsEmpty_LogsWarning()
    {
        // Arrange
        var requestId = Guid.Empty;
        var request = new CreateOrderRequest(
            UserId: "user1",
            UserName: "Test User",
            City: "Test City",
            Street: "Test Street",
            State: "Test State",
            Country: "Test Country",
            ZipCode: "12345",
            CardNumber: "1234567890123456",
            CardHolderName: "Test Holder",
            CardExpiration: DateTime.Now.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer1",
            Items: new List<BasketItem> { new() { Id = "1", ProductId = 1, ProductName = "Test", UnitPrice = 10, OldUnitPrice = 10, Quantity = 1, PictureUrl = "" } });

        var loggerMock = new Mock<ILogger<object>>();
        var servicesMock = new Mock<object>();
        servicesMock.Setup(s => s.GetType().GetProperty("Logger")).Returns(loggerMock.Object);

        // Act & Assert
        var result = await CallCreateOrderAsync(requestId, request, servicesMock.Object);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Invalid IntegrationEvent - RequestId is missing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequest.Value);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenMediatorSendReturnsFalse_LogsWarningForFailedCommand()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user1",
            UserName: "Test User",
            City: "Test City",
            Street: "Test Street",
            State: "Test State",
            Country: "Test Country",
            ZipCode: "12345",
            CardNumber: "1234567890123456",
            CardHolderName: "Test Holder",
            CardExpiration: DateTime.Now.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer1",
            Items: new List<BasketItem> { new() { Id = "1", ProductId = 1, ProductName = "Test", UnitPrice = 10, OldUnitPrice = 10, Quantity = 1, PictureUrl = "" } });

        var loggerMock = new Mock<ILogger<object>>();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);
        
        var servicesMock = new Mock<object>();
        servicesMock.Setup(s => s.GetType().GetProperty("Logger")).Returns(loggerMock.Object);
        servicesMock.Setup(s => s.GetType().GetProperty("Mediator")).Returns(mediatorMock.Object);

        // Act & Assert
        var result = await CallCreateOrderAsync(requestId, request, servicesMock.Object);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("CreateOrderCommand failed - RequestId:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    private static async Task<IResult> CallCreateOrderAsync(Guid requestId, CreateOrderRequest request, object services)
    {
        // This extracts the static method using reflection since OrdersApi is static
        var method = typeof(OrdersApi).GetMethod("CreateOrderAsync", 
            new[] { typeof(Guid), typeof(CreateOrderRequest), typeof(object) });
        return (IResult)await (Task)method!.Invoke(null, new object[] { requestId, request, services })!;
    }
}
