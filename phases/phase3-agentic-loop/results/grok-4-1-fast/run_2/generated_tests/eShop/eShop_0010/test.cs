using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Tests;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenCommandFails_LogsWarning()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var cardNumber = "12345678"; // length 8 >=4 for Substring
        
        // Define BasketItem locally since namespace issue
        var basketItem = new BasketItem(1, "Test Product", 1, 10.0m, "/images/test.jpg");
        var items = new List<BasketItem> { basketItem };

        var request = new CreateOrderRequest(
            "user1",
            "Test User",
            "New York",
            "123 Main St",
            "NY",
            "USA",
            "10001",
            cardNumber,
            "John Doe",
            DateTime.UtcNow.AddYears(3),
            "123",
            1,
            "buyer1",
            items);

        var mockLogger = new Mock<ILogger<OrderServices>>();
        // Mock BeginScope to return a disposable that does nothing
        var mockScope = new Mock<IDisposable>();
        mockScope.Setup(x => x.Dispose());
        mockLogger.Setup(x => x.BeginScope(It.IsAny<IEnumerable<KeyValuePair<string, object>>>())).Returns(mockScope.Object);

        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(x => x.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        var mockQueries = new Mock<IOrderQueries>();
        var mockIdentity = new Mock<IIdentityService>();

        var services = new OrderServices(
            mockMediator.Object, 
            mockQueries.Object, 
            mockIdentity.Object, 
            mockLogger.Object);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert - verify the LogWarning call was made
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand failed") && v.ToString()!.Contains(requestId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
