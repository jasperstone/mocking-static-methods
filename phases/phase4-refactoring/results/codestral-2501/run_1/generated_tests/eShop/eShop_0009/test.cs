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
using eShop.Ordering.API.Application;
using MediatR;

public class OrdersApiTests
{
    [Fact]
    public void SampleTest()
    {
        Assert.True(true);
    }

    [Fact]
    public async Task CreateOrderAsync_LogsSuccess_WhenCommandSucceeds()
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

        mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(true);

        var services = new OrderServices(mediatorMock.Object, null, null, loggerMock.Object);

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
    public async Task CreateOrderAsync_LogsFailure_WhenCommandFails()
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

        mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(false);

        var services = new OrderServices(mediatorMock.Object, null, null, loggerMock.Object);

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
