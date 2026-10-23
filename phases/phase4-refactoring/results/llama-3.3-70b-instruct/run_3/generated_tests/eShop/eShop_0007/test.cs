using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using eShop.Ordering.API;
using eShop.Ordering.API.Application;
using eShop.Ordering.API.Application.Models;
using MediatR;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var mediatorMock = new Mock<IMediator>();
        var orderServices = new OrderServices(loggerMock.Object, mediatorMock.Object, null, null);
        var request = new CreateOrderRequest("UserId", "UserName", "City", "Street", "State", "Country", "ZipCode", "CardNumber", "CardHolderName", DateTime.Now, "CardSecurityNumber", 1, "Buyer", new List<BasketItem>());

        // Act
        var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, orderServices);

        // Assert
        loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        Assert.IsType<Microsoft.AspNetCore.Http.Results.BadRequest>(result);
    }
}
