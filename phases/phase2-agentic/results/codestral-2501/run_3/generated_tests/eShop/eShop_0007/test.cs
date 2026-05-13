using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using eShop.Ordering.API.Extensions;
using MediatR;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();

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

        var services = new OrderServices
        {
            Logger = loggerMock.Object,
            Mediator = mediatorMock.Object
        };

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
    public async Task CreateOrderDraftAsync_LogsInformationAndSendsCommand()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderServices>>();
        var mediatorMock = new Mock<IMediator>();

        var command = new CreateOrderDraftCommand("buyerId", new List<BasketItem>());
        var services = new OrderServices
        {
            Logger = loggerMock.Object,
            Mediator = mediatorMock.Object
        };

        // Act
        var result = await OrdersApi.CreateOrderDraftAsync(command, services);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);

        mediatorMock.Verify(x => x.Send(command, default), Times.Once);
    }
}
