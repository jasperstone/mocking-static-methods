using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System.Collections.Generic;
using Xunit;
using MediatR;

namespace eShop.Ordering.API.Apis.Tests;

public class OrdersApiTests
{
    private readonly Mock<OrderServices> _mockServices;
    private readonly Mock<ILogger<OrderServices>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;

    public OrdersApiTests()
    {
        _mockLogger = new Mock<ILogger<OrderServices>>();
        _mockMediator = new Mock<IMediator>();
        _mockServices = new Mock<OrderServices>();
        _mockServices.SetupProperty(s => s.Logger, _mockLogger.Object);
        _mockServices.SetupProperty(s => s.Mediator, _mockMediator.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_SuccessfulCommandExecution_LogsSuccessMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user123",
            UserName: "John Doe",
            City: "New York",
            Street: "123 Main St",
            State: "NY",
            Country: "USA",
            ZipCode: "10001",
            CardNumber: "1234567890123456",
            CardHolderName: "John Doe",
            CardExpiration: DateTime.Now.AddYears(3),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer@test.com",
            Items: new List<eShop.Ordering.API.Application.Models.BasketItem>
            {
                new() { Id = "item1", ProductId = 1, ProductName = "Test Product", UnitPrice = 10.0m, Quantity = 1 }
            }
        );

        _mockMediator.Setup(s => s.Send(It.IsAny<object>()))
                    .ReturnsAsync(true);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _mockServices.Object);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand succeeded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_FailedCommandExecution_LogsWarningMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user123",
            UserName: "John Doe",
            City: "New York",
            Street: "123 Main St",
            State: "NY",
            Country: "USA",
            ZipCode: "10001",
            CardNumber: "1234567890123456",
            CardHolderName: "John Doe",
            CardExpiration: DateTime.Now.AddYears(3),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer@test.com",
            Items: new List<eShop.Ordering.API.Application.Models.BasketItem>()
        );

        _mockMediator.Setup(s => s.Send(It.IsAny<object>()))
                    .ReturnsAsync(false);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _mockServices.Object);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }
}
