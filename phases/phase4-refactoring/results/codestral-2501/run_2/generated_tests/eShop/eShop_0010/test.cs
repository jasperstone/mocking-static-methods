using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.Models;
using MediatR;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.Empty;
        var request = new CreateOrderRequest(
            "userId",
            "userName",
            "city",
            "street",
            "state",
            "country",
            "zipCode",
            "1234567890123456",
            "cardHolderName",
            DateTime.Now,
            "123",
            1,
            "buyer",
            new List<BasketItem>()
        );

        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();
        var queriesMock = new Mock<IOrderQueries>();
        var identityServiceMock = new Mock<IIdentityService>();

        var services = new OrderServices(
            mediatorMock.Object,
            queriesMock.Object,
            identityServiceMock.Object,
            loggerMock.Object
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
    public async Task CreateOrderAsync_ValidRequestId_LogsInformationAndReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            "userId",
            "userName",
            "city",
            "street",
            "state",
            "country",
            "zipCode",
            "1234567890123456",
            "cardHolderName",
            DateTime.Now,
            "123",
            1,
            "buyer",
            new List<BasketItem>()
        );

        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();
        var queriesMock = new Mock<IOrderQueries>();
        var identityServiceMock = new Mock<IIdentityService>();

        var services = new OrderServices(
            mediatorMock.Object,
            queriesMock.Object,
            identityServiceMock.Object,
            loggerMock.Object
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
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            "userId",
            "userName",
            "city",
            "street",
            "state",
            "country",
            "zipCode",
            "1234567890123456",
            "cardHolderName",
            DateTime.Now,
            "123",
            1,
            "buyer",
            new List<BasketItem>()
        );

        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();
        var queriesMock = new Mock<IOrderQueries>();
        var identityServiceMock = new Mock<IIdentityService>();

        var services = new OrderServices(
            mediatorMock.Object,
            queriesMock.Object,
            identityServiceMock.Object,
            loggerMock.Object
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
