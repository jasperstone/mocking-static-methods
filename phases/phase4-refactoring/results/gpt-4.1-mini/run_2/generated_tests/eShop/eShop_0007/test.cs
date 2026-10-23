using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Application.Commands;
using static Microsoft.AspNetCore.Http.TypedResults;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_LogsWarningAndReturnsBadRequest_WhenRequestIdIsEmpty()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();
        var queriesMock = new Mock<IOrderQueries>();
        var identityServiceMock = new Mock<IIdentityService>();

        var services = new OrderServices(mediatorMock.Object, queriesMock.Object, identityServiceMock.Object, loggerMock.Object);

        var request = new CreateOrderRequest(
            UserId: "user1",
            UserName: "User One",
            City: "City",
            Street: "Street",
            State: "State",
            Country: "Country",
            ZipCode: "12345",
            CardNumber: "1234567890123456",
            CardHolderName: "Holder",
            CardExpiration: DateTime.UtcNow.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "Buyer",
            Items: new List<BasketItem>());

        var emptyRequestId = Guid.Empty;

        // Act
        var result = await OrdersApi.CreateOrderAsync(emptyRequestId, request, services);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid IntegrationEvent - RequestId is missing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.IsType<BadRequest<string>>(result);
    }
}
