using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediatR;
using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.Domain.SeedWork;

namespace eShop.Ordering.API.Apis.Tests;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenRequestIdIsEmpty_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.Empty;
        var request = new CreateOrderRequest(
            "user123", "John Doe", "New York", "123 Main St", "NY", "USA", "10001",
            "1234567890123456", "John Doe", DateTime.Now.AddYears(3), "123", 1, "buyer1",
            new List<BasketItem>());

        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();
        var queriesMock = new Mock<IOrderQueries>();
        var identityMock = new Mock<IIdentityService>();

        var services = new OrderServices(mediatorMock.Object, queriesMock.Object, identityMock.Object, loggerMock.Object);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString().Contains("Invalid IntegrationEvent - RequestId is missing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequestResult.Value);
    }
}
