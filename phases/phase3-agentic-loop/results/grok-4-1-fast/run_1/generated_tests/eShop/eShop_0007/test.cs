using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;

namespace eShop.Ordering.API.Apis.Tests;

public class OrdersApiLoggerTests
{
    [Fact]
    public void CreateOrderAsync_EmptyRequestId_LogsWarningMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OrdersApi>>();
        var services = new MockOrderServices { Logger = mockLogger.Object };
        
        var emptyRequestId = Guid.Empty;
        var request = new CreateOrderRequest(
            "user123",
            "John Doe",
            "New York",
            "123 Main St",
            "NY",
            "USA",
            "10001",
            "1234567890123456",
            "John Doe",
            DateTime.Now.AddYears(3),
            "123",
            1,
            "buyer",
            new List<BasketItem> { new BasketItem("item1", 1, 10.0m, "", "") });

        // Act
        var result = OrdersApi.CreateOrderAsync(emptyRequestId, request, services).Result;

        // Assert - Verify LogWarning was called with correct parameters
        mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                "Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}",
                It.Is<object[]>(args => args.Length == 1 && ReferenceEquals(args[0], request))),
            Times.Once);
    }
}

// Test-specific mock classes to avoid static class issues
public class MockOrderServices : OrderServices
{
    public ILogger<OrdersApi> Logger { get; set; } = null!;
}

public record BasketItem(string Id, int Quantity, decimal UnitPrice, string PictureUrl, string ProductName);
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
