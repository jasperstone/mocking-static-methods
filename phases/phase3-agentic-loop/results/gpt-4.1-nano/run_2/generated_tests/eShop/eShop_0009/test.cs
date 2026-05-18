using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using static eShop.Ordering.API.Apis.OrdersApi;

public class OrdersApiTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrderServices _services;

    public OrdersApiTests()
    {
        _loggerMock = new Mock<ILogger>();
        _mediatorMock = new Mock<IMediator>();
        _services = new OrderServices
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object,
            Queries = null,
            IdentityService = null
        };
    }

    [Fact]
    public async Task CreateOrderAsync_LogsInformationAndReturnsOk_WhenResultIsTrue()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user1",
            UserName: "User One",
            City: "City",
            Street: "Street",
            State: "State",
            Country: "Country",
            ZipCode: "Zip",
            CardNumber: "1234567890123456",
            CardHolderName: "Holder",
            CardExpiration: DateTime.UtcNow.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "Buyer",
            Items: new List<BasketItem>());

        _services.Mediator.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
            .ReturnsAsync(true);

        // Act
        var result = await CreateOrderAsync(requestId, request, _services);

        // Assert
        var okResult = Assert.IsType<Results<Ok, BadRequest<string>>>(result);
        Assert.IsType<Ok>(okResult);
        _loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Sending command:")),
                It.IsAny<object[]>()),
            Times.AtLeastOnce);
        _loggerMock.Verify(
            x => x.LogInformation(
                "CreateOrderCommand succeeded - RequestId: {RequestId}",
                requestId),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_LogsWarningAndReturnsBadRequest_WhenRequestIdIsEmpty()
    {
        // Arrange
        var request = new CreateOrderRequest(
            UserId: "user1",
            UserName: "User One",
            City: "City",
            Street: "Street",
            State: "State",
            Country: "Country",
            ZipCode: "Zip",
            CardNumber: "1234567890123456",
            CardHolderName: "Holder",
            CardExpiration: DateTime.UtcNow.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "Buyer",
            Items: new List<BasketItem>());

        // Act
        var result = await CreateOrderAsync(Guid.Empty, request, _services);

        // Assert
        var badRequestResult = Assert.IsType<Results<Ok, BadRequest<string>>>(result);
        Assert.IsType<BadRequest<string>>(badRequestResult);
        _loggerMock.Verify(
            x => x.LogWarning(
                "Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}",
                request),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_LogsInformationAndReturnsOk_WhenResultIsFalse()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            UserId: "user1",
            UserName: "User One",
            City: "City",
            Street: "Street",
            State: "State",
            Country: "Country",
            ZipCode: "Zip",
            CardNumber: "1234567890123456",
            CardHolderName: "Holder",
            CardExpiration: DateTime.UtcNow.AddYears(1),
            CardSecurityNumber: "123",
            CardTypeId: 1,
            Buyer: "Buyer",
            Items: new List<BasketItem>());

        _services.Mediator.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
            .ReturnsAsync(false);

        // Act
        var result = await CreateOrderAsync(requestId, request, _services);

        // Assert
        var okResult = Assert.IsType<Results<Ok, BadRequest<string>>>(result);
        Assert.IsType<Ok>(okResult);
        _loggerMock.Verify(
            x => x.LogWarning(
                "CreateOrderCommand failed - RequestId: {RequestId}",
                requestId),
            Times.Once);
    }
}
