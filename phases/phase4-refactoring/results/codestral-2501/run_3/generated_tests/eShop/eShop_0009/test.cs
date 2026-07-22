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
using MediatR;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_LogsInformation_WhenCommandSucceeds()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var mediatorMock = new Mock<IMediator>();

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

        mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                    .ReturnsAsync(true);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, new OrderServices(mediatorMock.Object, null, null, loggerMock.Object));

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Exactly(2));

        Assert.IsType<Ok>(result);
    }
}
