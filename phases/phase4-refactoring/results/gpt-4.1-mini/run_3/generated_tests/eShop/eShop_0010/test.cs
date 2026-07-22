using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static Microsoft.AspNetCore.Http.TypedResults;
using Microsoft.AspNetCore.Http.HttpResults;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_LogsWarning_WhenRequestIdIsEmpty()
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
            UserId: "user1",
            UserName: "User One",
            City: "City",
            Street: "Street",
            State: "State",
            Country: "Country",
            ZipCode: "12345",
            CardNumber: "1234567890123456",
            CardHolderName: "Holder",
            CardExpiration: DateTime.UtcNow.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "Buyer",
            Items: new List<BasketItem>());

        // Act
        var result = await eShop.Ordering.API.OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid IntegrationEvent - RequestId is missing")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.IsType<BadRequest<string>>(result);
    }
}

// Minimal interfaces and classes to compile the test
public interface IMediator
{
    Task<TResponse> Send<TResponse>(object command);
}

public class OrderServices
{
    public ILogger Logger { get; set; }
    public IMediator Mediator { get; set; }
}

public record CreateOrderRequest(
    string UserId,
    string UserName,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string CardNumber,
    string CardHolderName,
    DateTime CardExpiration,
    string CardSecurityNumber,
    int CardTypeId,
    string Buyer,
    List<BasketItem> Items);

public class BasketItem { }
