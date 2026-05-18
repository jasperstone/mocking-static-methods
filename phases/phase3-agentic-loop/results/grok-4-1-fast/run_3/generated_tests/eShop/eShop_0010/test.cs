using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Common.Models;

namespace eShop.Ordering.API.Tests;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenMediatorReturnsFalse_LogsWarningWithRequestId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OrderServices>>();
        var mockMediator = new Mock<IMediator>();
        var mockQueries = new Mock<IOrderQueries>();
        var mockIdentity = new Mock<IIdentityService>();

        var services = new OrderServices(
            mockMediator.Object, 
            mockQueries.Object, 
            mockIdentity.Object, 
            mockLogger.Object);

        var basketItem = new BasketItem
        {
            Id = "item1",
            ProductId = 1,
            ProductName = "Test Product",
            UnitPrice = 10.0m,
            OldUnitPrice = 10.0m,
            Quantity = 1,
            PictureUrl = ""
        };

        var request = new CreateOrderRequest(
            UserId: "user123",
            UserName: "Test User",
            City: "Test City",
            Street: "Test Street",
            State: "Test State",
            Country: "Test Country",
            ZipCode: "12345",
            CardNumber: "1234567890123456",
            CardHolderName: "Test Holder",
            CardExpiration: DateTime.Now.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer@test.com",
            Items: new List<BasketItem> { basketItem }
        );

        var requestId = Guid.NewGuid();

        mockMediator
            .Setup(m => m.Send(
                It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert - Verify the specific LogWarning call was made
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, t) => 
                    value.ToString().Contains("CreateOrderCommand failed") && 
                    value.ToString().Contains(requestId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
