using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using System.Collections.Generic;

namespace eShop.Ordering.API.UnitTests.Apis;

public class OrdersApiTests
{
    private readonly Mock<OrderServices> _mockServices;
    private readonly Mock<ILogger<OrderServices>> _mockLogger;

    public OrdersApiTests()
    {
        _mockLogger = new Mock<ILogger<OrderServices>>();
        Mock<IMediator> mockMediator = new();
        Mock<IOrderQueries> mockQueries = new();
        Mock<IIdentityService> mockIdentity = new();
        
        _mockServices = new Mock<OrderServices>(
            mockMediator.Object,
            mockQueries.Object,
            mockIdentity.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyRequestId_LogsSpecificWarningAndReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.Empty;
        var request = new CreateOrderRequest(
            "user123",
            "John Doe",
            "New York",
            "123 Main St",
            "NY",
            "USA",
            "10001",
            "4111111111111111",
            "John Doe",
            DateTime.Now.AddYears(3),
            "123",
            1,
            "buyer",
            new List<BasketItem> { new BasketItem() });

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _mockServices.Object);

        // Assert - Verify the specific LogWarning call on line 134
        _mockLogger.Verify(
            x => x.LogWarning(
                "Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}",
                request),
            Times.Once);

        var badRequestResult = Assert.IsType<BadRequest<string>>(result.Value);
        Assert.Equal("RequestId is missing.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequestId_DoesNotLogMissingRequestIdWarning()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            "user123",
            "John Doe",
            "New York",
            "123 Main St",
            "NY",
            "USA",
            "10001",
            "4111111111111111",
            "John Doe",
            DateTime.Now.AddYears(3),
            "123",
            1,
            "buyer",
            new List<BasketItem> { new BasketItem() });

        _mockServices.Setup(x => x.Mediator.Send(It.IsAny<object>())).ReturnsAsync(true);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, request, _mockServices.Object);

        // Assert - Verify the specific LogWarning for missing RequestId is NOT called
        _mockLogger.Verify(
            x => x.LogWarning(
                "Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}",
                It.IsAny<CreateOrderRequest>()),
            Times.Never);

        Assert.IsType<Ok>(result.Value);
    }
}

// Test support types - copied from source
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

public record BasketItem();

public class OrderServices
{
    public IMediator Mediator { get; set; }
    public ILogger<OrderServices> Logger { get; }
    public IOrderQueries Queries { get; }
    public IIdentityService IdentityService { get; }

    public OrderServices(IMediator mediator, IOrderQueries queries, IIdentityService identityService, ILogger<OrderServices> logger)
    {
        Mediator = mediator;
        Logger = logger;
        Queries = queries;
        IdentityService = identityService;
    }
}

public interface IMediator 
{ 
    Task<bool> Send<T>(IdentifiedCommand<T, bool> command); 
    Task<object> Send(object command); 
}
public interface IOrderQueries { }
public interface IIdentityService { }

public class IdentifiedCommand<T, TResult>
{
    public T Command { get; }
    public Guid Id { get; }

    public IdentifiedCommand(T command, Guid id)
    {
        Command = command;
        Id = id;
    }
}
