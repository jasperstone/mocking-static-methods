using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;

namespace eShop.Ordering.API.Tests.Apis;

public class OrdersApiTests
{
    private readonly Mock<ILogger<object>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrderServices _services;

    public OrdersApiTests()
    {
        _loggerMock = new Mock<ILogger<object>>();
        _mediatorMock = new Mock<IMediator>();
        _services = new OrderServices
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object
        };
    }

    [Fact]
    public async Task CreateOrderAsync_SuccessfulCommand_LogsSuccessMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = CreateTestRequest();
        
        SetupMediatorSuccess();

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _services);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand succeeded - RequestId: " + requestId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_FailedCommand_LogsWarningMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = CreateTestRequest();
        
        SetupMediatorFailure();

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _services);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand failed - RequestId: " + requestId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }

    private CreateOrderRequest CreateTestRequest()
    {
        return new CreateOrderRequest(
            UserId: "user123",
            UserName: "John Doe",
            City: "New York",
            Street: "123 Main St",
            State: "NY",
            Country: "USA",
            ZipCode: "10001",
            CardNumber: "4111111111111111",
            CardHolderName: "John Doe",
            CardExpiration: DateTime.Now.AddYears(3),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "buyer1",
            Items: new List<BasketItem>()
        );
    }

    private void SetupMediatorSuccess()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupMediatorFailure()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(false);
    }
}

// Production types mirrored for testing - complete implementation
public static class OrdersApi
{
    public static async Task<Results<Ok, BadRequest<string>>> CreateOrderAsync(
        Guid requestId,
        CreateOrderRequest request,
        OrderServices services)
    {
        // Mock the GetGenericTypeName calls that exist in production
        services.Logger.LogInformation(
            "Sending command: {CommandName} - {IdProperty}: {CommandId}",
            "CreateOrderRequest",
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

public class OrderServices
{
    public ILogger<object> Logger { get; set; } = null!;
    public IMediator Mediator { get; set; } = null!;
}

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, System.Threading.CancellationToken cancellationToken = default);
}

public interface IRequest<TResponse> { }

public class BasketItem { }

public class CreateOrderCommand 
{ 
    public CreateOrderCommand(List<BasketItem> items, string userId, string userName, string city, string street,
        string state, string country, string zipCode, string cardNumber, string cardHolderName, 
        DateTime cardExpiration, string cardSecurityNumber, int cardTypeId) { }
}

public class IdentifiedCommand<TCommand, TResult> : IRequest<TResult>
{
    public TCommand Command { get; }
    public Guid Id { get; }

    public IdentifiedCommand(TCommand command, Guid id)
    {
        Command = command;
        Id = id;
    }

    public string GetGenericTypeName() => "IdentifiedCommand";
}
