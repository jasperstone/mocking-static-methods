using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eShop.Ordering.API.Apis.Tests;

public class OrdersApiTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenRequestIdIsEmpty_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var emptyRequestId = Guid.Empty;
        var request = new CreateOrderRequest(
            "user1", "Test User", "Test City", "123 Test St", "TS", "USA", "12345",
            "1234567890123456", "Test Holder", DateTime.Now.AddYears(1), "123", 1, "buyer1",
            new List<BasketItem>());

        var logger = new Mock<ILogger<OrdersApi>>();
        var mediator = new Mock<IMediator>();
        var services = new OrderServices(logger.Object, mediator.Object, Mock.Of<IIdentityService>(), Mock.Of<IOrderQueries>());

        // Act
        var result = await OrdersApi.CreateOrderAsync(emptyRequestId, request, services);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Invalid IntegrationEvent - RequestId is missing") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenMediatorReturnsFalse_LogsWarningWithRequestId()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            "user1", "Test User", "Test City", "123 Test St", "TS", "USA", "12345",
            "1234567890123456", "Test Holder", DateTime.Now.AddYears(1), "123", 1, "buyer1",
            new List<BasketItem>());

        var logger = new Mock<ILogger<OrdersApi>>();
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        var services = new OrderServices(logger.Object, mediator.Object, Mock.Of<IIdentityService>(), Mock.Of<IOrderQueries>());

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("CreateOrderCommand failed - RequestId: " + requestId) == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.IsType<Ok>(result);
    }
}

// Supporting types to compile
public record BasketItem();

public class OrderServices
{
    public ILogger<OrdersApi> Logger { get; }
    public IMediator Mediator { get; }
    public IIdentityService IdentityService { get; }
    public IOrderQueries Queries { get; }

    public OrderServices(ILogger<OrdersApi> logger, IMediator mediator, IIdentityService identityService, IOrderQueries queries)
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

public interface IRequest<out T> { }

public interface IIdentityService
{
    string GetUserIdentity();
}

public interface IOrderQueries
{
    Task<object> GetOrderAsync(int orderId);
    Task<IEnumerable<object>> GetOrdersFromUserAsync(string userId);
    Task<IEnumerable<object>> GetCardTypesAsync();
}

public class CreateOrderCommand
{
    public CreateOrderCommand(
        List<BasketItem> items, string userId, string userName, string city, string street,
        string state, string country, string zipCode, string cardNumber, string cardHolderName,
        DateTime cardExpiration, string cardSecurityNumber, int cardTypeId)
    {
    }
}

public class IdentifiedCommand<T, TResult> : IRequest<TResult>
{
    public IdentifiedCommand(T command, Guid id) { }
    public Guid Id { get; } = Guid.NewGuid();
    public T Command { get; } = default!;
    public string GetGenericTypeName() => typeof(T).Name;
}
