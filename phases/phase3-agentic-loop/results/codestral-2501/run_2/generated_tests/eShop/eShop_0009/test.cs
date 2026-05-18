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
using eShop.Ordering.API.Apis;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_LogsInformation_WhenRequestIdIsValid()
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

        mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(true);

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
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand succeeded")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_LogsWarning_WhenRequestIdIsInvalid()
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
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid IntegrationEvent")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.IsType<BadRequest<string>>(result);
    }
}
