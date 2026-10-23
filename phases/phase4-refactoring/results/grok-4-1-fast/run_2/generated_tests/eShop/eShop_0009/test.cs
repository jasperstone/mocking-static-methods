using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

namespace eShop.Ordering.API.UnitTests.Apis;

public class OrdersApiTests
{
    private readonly Mock<ILogger<OrdersApi>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrderServices _services;

    public OrdersApiTests()
    {
        _loggerMock = new Mock<ILogger<OrdersApi>>();
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _mediatorMock = new Mock<IMediator>();
        _services = new OrderServices
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object
        };
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCommandSucceeds_LogsSuccessMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = CreateTestRequest();
        
        SetupMediatorSuccess(requestId);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _services);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("CreateOrderCommand succeeded - RequestId:") && s.Contains(requestId.ToString())),
                It.IsAny<object[]>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCommandFails_LogsWarningMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = CreateTestRequest();
        
        SetupMediatorFailure(requestId);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _services);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("CreateOrderCommand failed - RequestId:") && s.Contains(requestId.ToString())),
                It.IsAny<object[]>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenRequestIdEmpty_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.Empty;
        var request = CreateTestRequest();

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _services);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning(
                "Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}",
                It.IsAny<object[]>()),
            Times.Once);

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequest.Value);
    }

    private CreateOrderRequest CreateTestRequest()
    {
        return new CreateOrderRequest(
            UserId: "user1",
            UserName: "John Doe",
            City: "New York",
            Street: "123 Main St",
            State: "NY",
            Country: "USA",
            ZipCode: "10001",
            CardNumber: "1234567890123456",
            CardHolderName: "John Doe",
            CardExpiration: DateTime.UtcNow.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer1",
            Items: new List<BasketItem>()
        );
    }

    private void SetupMediatorSuccess(Guid requestId)
    {
        var createOrderCommand = new CreateOrderCommand(
            new List<BasketItem>(), "user1", "John Doe", "New York", "123 Main St",
            "NY", "USA", "10001", "XXXXXXXXXXXX3456", "John Doe",
            DateTime.UtcNow.AddYears(1), "123", 1);

        var identifiedCommand = new IdentifiedCommand<CreateOrderCommand, bool>(createOrderCommand, requestId);
        
        _mediatorMock.Setup(m => m.Send(identifiedCommand, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private void SetupMediatorFailure(Guid requestId)
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);
    }
}

// Minimal implementations for compilation
public class OrderServices
{
    public ILogger<OrdersApi> Logger { get; set; } = NullLogger<OrdersApi>.Instance;
    public IMediator Mediator { get; set; } = null!;
}

public record BasketItem();

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

public class CreateOrderCommand
{
    public List<BasketItem> Items { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string CardNumber { get; set; } = null!;
    public string CardHolderName { get; set; } = null!;
    public DateTime CardExpiration { get; set; }
    public string CardSecurityNumber { get; set; } = null!;
    public int CardTypeId { get; set; }

    public CreateOrderCommand(List<BasketItem> items, string userId, string userName, string city, string street,
        string state, string country, string zipCode, string cardNumber, string cardHolderName,
        DateTime cardExpiration, string cardSecurityNumber, int cardTypeId)
    {
        Items = items;
        UserId = userId;
        UserName = userName;
        City = city;
        Street = street;
        State = state;
        Country = country;
        ZipCode = zipCode;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        CardExpiration = cardExpiration;
        CardSecurityNumber = cardSecurityNumber;
        CardTypeId = cardTypeId;
    }
}

public class IdentifiedCommand<T, TResult>
{
    public IdentifiedCommand(T command, Guid id)
    {
        Command = command;
        Id = id;
    }

    public T Command { get; }
    public Guid Id { get; }

    public string GetGenericTypeName() => typeof(T).Name;
}

public static class OrdersApi
{
    public static async Task<Results<Ok, BadRequest<string>>> CreateOrderAsync(
        Guid requestId,
        CreateOrderRequest request,
        OrderServices services)
    {
        services.Logger.LogInformation(
            "Sending command: {CommandName} - {IdProperty}: {CommandId}",
            request.GetGenericTypeName(),
            nameof(request.UserId),
            request.UserId);

        if (requestId == Guid.Empty)
        {
            services.Logger.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request);
            return TypedResults.BadRequest("RequestId is missing.");
        }

        using (services.Logger.BeginScope(new List<KeyValuePair<string, object>> { new("IdentifiedCommandId", requestId) }))
        {
            var maskedCCNumber = request.CardNumber.Substring(request.CardNumber.Length - 4).PadLeft(request.CardNumber.Length, 'X');
            var createOrderCommand = new CreateOrderCommand(request.Items, request.UserId, request.UserName, request.City, request.Street,
                request.State, request.Country, request.ZipCode,
                maskedCCNumber, request.CardHolderName, request.CardExpiration,
                request.CardSecurityNumber, request.CardTypeId);

            var requestCreateOrder = new IdentifiedCommand<CreateOrderCommand, bool>(createOrderCommand, requestId);

            services.Logger.LogInformation(
                "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                requestCreateOrder.GetGenericTypeName(),
                nameof(requestCreateOrder.Id),
                requestCreateOrder.Id,
                requestCreateOrder);

            var result = await services.Mediator.Send(requestCreateOrder);

            if (result)
            {
                services.Logger.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestId);
            }
            else
            {
                services.Logger.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId);
            }

            return TypedResults.Ok();
        }
    }
}

public static class Extensions
{
    public static string GetGenericTypeName<T>(this T obj) => typeof(T).Name;
}
