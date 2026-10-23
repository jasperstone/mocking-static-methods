using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;

namespace eShop.Ordering.API.UnitTests;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_SuccessfulCommand_LogsSuccessMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user123",
            UserName: "John Doe",
            City: "New York",
            Street: "123 Main St",
            State: "NY",
            Country: "USA",
            ZipCode: "10001",
            CardNumber: "1234567890123456",
            CardHolderName: "John Doe",
            CardExpiration: DateTime.Now.AddYears(3),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer123",
            Items: new List<object>());

        var mockMediator = new Mock<IMediator>();
        mockMediator
            .Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockLogger = new Mock<ILogger<eShop.Ordering.API.Apis.OrderServices>>();
        var services = new eShop.Ordering.API.Apis.OrderServices(mockMediator.Object, null!, null!, mockLogger.Object);

        // Act
        var result = await eShop.Ordering.API.Apis.OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand succeeded - RequestId: " + requestId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_FailedCommand_LogsWarningMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user123",
            UserName: "John Doe",
            City: "New York",
            Street: "123 Main St",
            State: "NY",
            Country: "USA",
            ZipCode: "10001",
            CardNumber: "1234567890123456",
            CardHolderName: "John Doe",
            CardExpiration: DateTime.Now.AddYears(3),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer123",
            Items: new List<object>());

        var mockMediator = new Mock<IMediator>();
        mockMediator
            .Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var mockLogger = new Mock<ILogger<eShop.Ordering.API.Apis.OrderServices>>();
        var services = new eShop.Ordering.API.Apis.OrderServices(mockMediator.Object, null!, null!, mockLogger.Object);

        // Act
        var result = await eShop.Ordering.API.Apis.OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand failed - RequestId: " + requestId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_EmptyRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.Empty;
        var request = new CreateOrderRequest(
            UserId: "user123",
            UserName: "John Doe",
            City: "New York",
            Street: "123 Main St",
            State: "NY",
            Country: "USA",
            ZipCode: "10001",
            CardNumber: "1234567890123456",
            CardHolderName: "John Doe",
            CardExpiration: DateTime.Now.AddYears(3),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer123",
            Items: new List<object>());

        var mockLogger = new Mock<ILogger<eShop.Ordering.API.Apis.OrderServices>>();
        var services = new eShop.Ordering.API.Apis.OrderServices(null!, null!, null!, mockLogger.Object);

        // Act
        var result = await eShop.Ordering.API.Apis.OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid IntegrationEvent - RequestId is missing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequest.Value);
    }
}
