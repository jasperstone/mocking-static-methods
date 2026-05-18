using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_LogsInformation_WhenCommandSucceeds()
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
            new List<BasketItem>());

        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();

        mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(true);

        var services = new OrderServices
        {
            Logger = loggerMock.Object,
            Mediator = mediatorMock.Object
        };

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand succeeded")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_LogsWarning_WhenCommandFails()
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
            new List<BasketItem>());

        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();

        mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(false);

        var services = new OrderServices
        {
            Logger = loggerMock.Object,
            Mediator = mediatorMock.Object
        };

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand failed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);

        Assert.IsType<Ok>(result);
    }
}
