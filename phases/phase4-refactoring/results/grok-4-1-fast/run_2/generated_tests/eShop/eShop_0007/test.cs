using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace eShop.Ordering.API.Tests.Apis;

public class OrdersApiTests
{
    private readonly Mock<OrderServices> _mockServices;
    private readonly List<string> _loggedWarnings;

    public OrdersApiTests()
    {
        _loggedWarnings = new List<string>();
        var mockLogger = new Mock<ILogger<OrdersApi>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Warning),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
            .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, id, state, ex, formatter) =>
            {
                if (level == LogLevel.Warning)
                {
                    _loggedWarnings.Add(formatter(state, ex));
                }
            });

        _mockServices = new Mock<OrderServices>();
        _mockServices.Setup(s => s.Logger).Returns(mockLogger.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var emptyRequestId = Guid.Empty;
        var request = new CreateOrderRequest(
            "user123",
            "John Doe",
            "New York",
            "123 Main St",
            "NY",
            "USA",
            "10001",
            "4111111111111111",
            "John Doe",
            DateTime.Now.AddYears(3),
            "123",
            1,
            "buyer",
            new List<BasketItem>());

        // Act
        var result = await eShop.Ordering.API.Apis.OrdersApi.CreateOrderAsync(emptyRequestId, request, _mockServices.Object);

        // Assert
        Assert.Single(_loggedWarnings);
        Assert.Contains("Invalid IntegrationEvent - RequestId is missing", _loggedWarnings[0]);
        Assert.Contains("CreateOrderRequest", _loggedWarnings[0]);

        var badRequestResult = Assert.IsType<Results<Ok, BadRequest<string>>>(result);
        Assert.IsType<BadRequest<string>>(badRequestResult.Value);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequestId_DoesNotLogWarning()
    {
        // Arrange
        var validRequestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            "user123",
            "John Doe",
            "New York",
            "123 Main St",
            "NY",
            "USA",
            "10001",
            "4111111111111111",
            "John Doe",
            DateTime.Now.AddYears(3),
            "123",
            1,
            "buyer",
            new List<BasketItem>());

        _mockServices.Setup(s => s.Mediator.Send(It.IsAny<object>()))
            .ReturnsAsync(true);

        // Act
        var result = await eShop.Ordering.API.Apis.OrdersApi.CreateOrderAsync(validRequestId, request, _mockServices.Object);

        // Assert
        Assert.Empty(_loggedWarnings);

        var okResult = Assert.IsType<Results<Ok, BadRequest<string>>>(result);
        Assert.IsType<Ok>(okResult.Value);
    }
}

// Test doubles matching production types
public record CreateOrderRequest(
    string UserId,
    string UserName,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string CardNumber,
    string CardHolderName,
    DateTime CardExpiration,
    string CardSecurityNumber,
    int CardTypeId,
    string Buyer,
    List<BasketItem> Items);

public record BasketItem();

public class OrderServices
{
    public ILogger<OrdersApi> Logger { get; set; } = NullLogger<OrdersApi>.Instance;
    public object Mediator { get; set; } = null!;
}
