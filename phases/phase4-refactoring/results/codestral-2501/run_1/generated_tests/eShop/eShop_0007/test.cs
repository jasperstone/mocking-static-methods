using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrdersApi>>();
        var mediatorMock = new Mock<IMediator>();
        var services = new OrderServices
        {
            Logger = loggerMock.Object,
            Mediator = mediatorMock.Object
        };

        var requestId = Guid.Empty;
        var request = new CreateOrderRequest(
            "userId",
            "userName",
            "city",
            "street",
            "state",
            "country",
            "zipCode",
            "cardNumber",
            "cardHolderName",
            DateTime.Now,
            "cardSecurityNumber",
            1,
            "buyer",
            new List<BasketItem>()
        );

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);

        Assert.IsType<BadRequest<string>>(result);
    }
}
