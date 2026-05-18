using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;
using eShop.Ordering.API.Application.Queries;
using MediatR;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();
        var identityServiceMock = new Mock<IIdentityService>();
        var orderQueriesMock = new Mock<IOrderQueries>();

        var services = new OrderServices(
            mediatorMock.Object,
            orderQueriesMock.Object,
            identityServiceMock.Object,
            loggerMock.Object
        );

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

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_LogsInformationAndReturnsOk()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();
        var identityServiceMock = new Mock<IIdentityService>();
        var orderQueriesMock = new Mock<IOrderQueries>();

        var services = new OrderServices(
            mediatorMock.Object,
            orderQueriesMock.Object,
            identityServiceMock.Object,
            loggerMock.Object
        );

        var requestId = Guid.NewGuid();
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

        mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(true);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.AtLeastOnce);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_CommandFails_LogsWarningAndReturnsOk()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();
        var identityServiceMock = new Mock<IIdentityService>();
        var orderQueriesMock = new Mock<IOrderQueries>();

        var services = new OrderServices(
            mediatorMock.Object,
            orderQueriesMock.Object,
            identityServiceMock.Object,
            loggerMock.Object
        );

        var requestId = Guid.NewGuid();
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

        mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(false);

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

        Assert.IsType<Ok>(result);
    }
}
