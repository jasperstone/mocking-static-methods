using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
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
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid IntegrationEvent - RequestId is missing")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequestId_LogsInformationAndReturnsOk()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var mediatorMock = new Mock<IMediator>();
        var services = new OrderServices
        {
            Logger = loggerMock.Object,
            Mediator = mediatorMock.Object
        };

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
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand succeeded")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_CommandFails_LogsWarningAndReturnsOk()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var mediatorMock = new Mock<IMediator>();
        var services = new OrderServices
        {
            Logger = loggerMock.Object,
            Mediator = mediatorMock.Object
        };

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
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand failed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.IsType<Ok>(result);
    }
}
