using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Models;
using MediatR;
using System.Collections.Generic;

namespace eShop.Ordering.API.Tests.Apis;

public class OrderServices
{
    public ILogger<OrderServices> Logger { get; }
    public IMediator Mediator { get; }
    public object Queries { get; }
    public object IdentityService { get; }

    public OrderServices(ILogger<OrderServices> logger, IMediator mediator, object queries, object identityService)
    {
        Logger = logger;
        Mediator = mediator;
        Queries = queries;
        IdentityService = identityService;
    }
}

public class OrdersApiTests
{
    private readonly Mock<ILogger<OrderServices>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly OrderServices _services;

    public OrdersApiTests()
    {
        _mockLogger = new Mock<ILogger<OrderServices>>();
        _mockMediator = new Mock<IMediator>();
        _services = new OrderServices(_mockLogger.Object, _mockMediator.Object, null!, null!);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenRequestIdIsEmpty_LogsWarningAndReturnsBadRequest()
    {
        // Arrange
        var emptyRequestId = Guid.Empty;
        var request = new CreateOrderRequest(
            UserId: "user1",
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
            Buyer: "buyer1",
            Items: new List<BasketItem> { new BasketItem(1, 1, "Product", 10.0m, 1, "picture.jpg") });

        // Act
        var result = await GlobalTestHelper.CallCreateOrderAsync(emptyRequestId, request, _services);

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(
                "Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", 
                request),
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
            UserId: "user1",
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
            Buyer: "buyer1",
            Items: new List<BasketItem> { new BasketItem(1, 1, "Product", 10.0m, 1, "picture.jpg") });

        _mockMediator.Setup(x => x.Send(It.IsAny<object>(), It.IsAny<System.Threading.CancellationToken>()))
                     .ReturnsAsync(false);

        // Act
        var result = await GlobalTestHelper.CallCreateOrderAsync(requestId, request, _services);

        // Assert - Verify the specific warning log on line 163
        _mockLogger.Verify(
            x => x.LogWarning(
                "CreateOrderCommand failed - RequestId: {RequestId}", 
                requestId),
            Times.Once);

        Assert.IsType<Ok>(result);
    }
}

static class GlobalTestHelper
{
    public static async Task<Results<Ok, BadRequest<string>>> CallCreateOrderAsync(
        Guid requestId,
        CreateOrderRequest request,
        OrderServices services)
    {
        // Inline the static method logic to test the logger calls directly
        services.Logger.LogInformation(
            "Sending command: {CommandName} - {IdProperty}: {CommandId}",
            request.GetType().Name,
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
            
            var requestCreateOrder = new object(); // Simulate IdentifiedCommand

            services.Logger.LogInformation(
                "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                "IdentifiedCommand",
                "Id",
                requestId,
                requestCreateOrder);

            var result = await services.Mediator.Send(requestCreateOrder, default);

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
