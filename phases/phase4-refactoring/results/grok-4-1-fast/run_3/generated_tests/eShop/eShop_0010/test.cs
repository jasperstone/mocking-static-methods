using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace eShop.Ordering.API.Tests;

public class OrdersApiTests
{
    private static readonly CreateOrderRequest _validRequest = new(
        UserId: "user123",
        UserName: "Test User",
        City: "Test City",
        Street: "123 Test St",
        State: "TS",
        Country: "USA",
        ZipCode: "12345",
        CardNumber: "1234567890123456",
        CardHolderName: "Test Cardholder",
        CardExpiration: DateTime.Now.AddYears(1),
        CardSecurityNumber: "123",
        CardTypeId: 1,
        Buyer: "buyer@test.com",
        Items: new List<BasketItem>());

    [Fact]
    public async Task CreateOrderAsync_WithEmptyRequestId_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<object>>();
        var mediatorMock = new Mock<IMediator>();
        var orderServices = new OrderServices(loggerMock.Object, mediatorMock.Object, null!, null!);

        // Act
        var result = await CallCreateOrderAsync(Guid.Empty, _validRequest, orderServices);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid IntegrationEvent - RequestId is missing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequest.Value);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_FailurePath_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<object>>();
        var mediatorMock = new Mock<IMediator>();
        var requestId = Guid.NewGuid();
        var orderServices = new OrderServices(loggerMock.Object, mediatorMock.Object, null!, null!);

        mediatorMock
            .Setup(x => x.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await CallCreateOrderAsync(requestId, _validRequest, orderServices);

        // Assert - Verify the specific LogWarning call on line 163
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand failed") && v.ToString()!.Contains(requestId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_SuccessPath_LogsInformation_NotWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<object>>();
        var mediatorMock = new Mock<IMediator>();
        var requestId = Guid.NewGuid();
        var orderServices = new OrderServices(loggerMock.Object, mediatorMock.Object, null!, null!);

        mediatorMock
            .Setup(x => x.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await CallCreateOrderAsync(requestId, _validRequest, orderServices);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
        
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrderCommand succeeded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        Assert.IsType<Ok>(result);
    }

    private static async Task<Results<Ok, BadRequest<string>>> CallCreateOrderAsync(
        Guid requestId, 
        CreateOrderRequest request, 
        OrderServices services)
    {
        // Direct call to the static method - this simulates the actual code path
        return await eShop.Ordering.API.Apis.OrdersApi.CreateOrderAsync(requestId, request, services);
    }
}

// Test doubles - matching real project structure
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

public class OrderServices
{
    public ILogger Logger { get; }
    public IMediator Mediator { get; }
    public IIdentityService IdentityService { get; }
    public IOrderQueries Queries { get; }

    public OrderServices(ILogger logger, IMediator mediator, IIdentityService identityService, IOrderQueries queries)
    {
        Logger = logger;
        Mediator = mediator;
        IdentityService = identityService;
        Queries = queries;
    }
}

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

public interface IRequest<T> { }

public interface IIdentityService { string GetUserIdentity(); }

public interface IOrderQueries { }

public class IdentifiedCommand<TCommand, TResult> : IRequest<TResult>
    where TCommand : IRequest<TResult>
{
    public TCommand Command { get; }
    public Guid Id { get; }

    public IdentifiedCommand(TCommand command, Guid id)
    {
        Command = command;
        Id = id;
    }
}

public class CreateOrderCommand : IRequest<bool>
{
    public IEnumerable<object> OrderItems => new List<object>();
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public DateTime CardExpiration { get; set; }
    public string CardSecurityNumber { get; set; } = string.Empty;
    public int CardTypeId { get; set; }

    public CreateOrderCommand() { }
}
