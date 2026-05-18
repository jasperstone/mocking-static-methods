using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using eShop.Ordering.API.Application.Models;
using MediatR;
using System.Collections.Generic;
using System;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenRequestIdIsEmpty_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.Empty;
        var basketItems = new List<BasketItem> 
        { 
            new BasketItem 
            { 
                ProductId = 1, 
                ProductName = "Test Product", 
                UnitPrice = 10m, 
                Quantity = 1 
            } 
        };

        var request = new Apis.CreateOrderRequest(
            "user123", "John Doe", "New York", "123 Main St", "NY", "USA", "10001",
            "1234567890123456", "John Doe", DateTime.Now.AddYears(3), "123", 1, "buyer1",
            basketItems);

        var mockLogger = new Mock<ILogger<Apis.OrderServices>>();
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<MediatR.Request>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var services = new Apis.OrderServices(mockMediator.Object, null!, null!, mockLogger.Object);

        // Act
        var result = await Apis.OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state?.ToString()?.Contains("Invalid IntegrationEvent - RequestId is missing") == true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((func, t) => 
                    {
                        try 
                        {
                            var expectedRequest = request;
                            var state = ((IReadOnlyList<KeyValuePair<string, object?>>)func.Invoke(new object(), null!));
                            var integrationEvent = state.FirstOrDefault(kvp => kvp.Key == "{@IntegrationEvent}").Value;
                            return integrationEvent == expectedRequest;
                        }
                        catch
                        {
                            return false;
                        }
                    })),
            Times.Once);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequestResult.Value);
    }
}
