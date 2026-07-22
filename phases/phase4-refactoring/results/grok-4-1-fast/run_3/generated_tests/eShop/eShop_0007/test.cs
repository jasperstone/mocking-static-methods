using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace eShop.Ordering.API.Tests.Apis;

public class OrdersApiTests
{
    private readonly Mock<OrderServices> _mockServices;
    private readonly List<string> _warningMessages;

    public OrdersApiTests()
    {
        _warningMessages = new();
        
        var mockLogger = new Mock<ILogger<OrdersApi>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Warning),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, eventId, state, ex, formatter) =>
                _warningMessages.Add(formatter(state, ex)));
        
        mockLogger.Setup(x => x.BeginScope(It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
            .Returns((IDisposable)new Mock<IDisposable>().Object);
        
        _mockServices = new Mock<OrderServices>();
        _mockServices.Setup(s => s.Logger).Returns(mockLogger.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var emptyRequestId = Guid.Empty;
        var request = new CreateOrderRequest(
            "user123", "John Doe", "New York", "123 Main St", "NY", "USA", "10001",
            "4111111111111111", "John Doe", DateTime.Now.AddYears(3), "123", 1, "buyer",
            new List<BasketItem> { new() });

        // Act
        var result = await eShop.Ordering.API.Apis.OrdersApi.CreateOrderAsync(emptyRequestId, request, _mockServices.Object);

        // Assert - Verify the specific LogWarning call (line 134)
        Assert.Single(_warningMessages);
        Assert.Contains("Invalid IntegrationEvent - RequestId is missing", _warningMessages[0]);
        
        var badRequestResult = Assert.IsType<BadRequest<string>>(result.Value);
        Assert.Equal("RequestId is missing.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequestId_DoesNotLogRequestIdMissingWarning()
    {
        // Arrange
        var validRequestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            "user123", "John Doe", "New York", "123 Main St", "NY", "USA", "10001",
            "4111111111111111", "John Doe", DateTime.Now.AddYears(3), "123", 1, "buyer",
            new List<BasketItem> { new() });

        _mockServices.Setup(s => s.Mediator.Send(It.IsAny<object>())).ReturnsAsync(true);

        // Act
        var result = await eShop.Ordering.API.Apis.OrdersApi.CreateOrderAsync(validRequestId, request, _mockServices.Object);

        // Assert - No "RequestId is missing" warning logged
        Assert.DoesNotContain("Invalid IntegrationEvent - RequestId is missing", _warningMessages);
        Assert.IsType<Ok>(result.Value);
    }
}

// Minimal types for compilation
public record CreateOrderRequest(
    string UserId, string UserName, string City, string Street, string State, string Country,
    string ZipCode, string CardNumber, string CardHolderName, DateTime CardExpiration,
    string CardSecurityNumber, int CardTypeId, string Buyer, List<BasketItem> Items);

public record BasketItem();

public class OrderServices
{
    public ILogger<OrdersApi> Logger { get; set; } = NullLogger<OrdersApi>.Instance;
    public object Mediator { get; set; } = null!;
}
