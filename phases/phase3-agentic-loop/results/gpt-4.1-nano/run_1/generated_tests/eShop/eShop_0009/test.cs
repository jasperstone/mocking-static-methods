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
    private readonly Mock<IOrderQueries> _queriesMock;
    private readonly Mock<IIdentityService> _identityServiceMock;

    public OrdersApiTests()
    {
        _loggerMock = new Mock<ILogger>();
        _mediatorMock = new Mock<IMediator>();
        _queriesMock = new Mock<IOrderQueries>();
        _identityServiceMock = new Mock<IIdentityService>();
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

        var services = new
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default)).ReturnsAsync(true);

        // Act
        var result = await CreateOrderAsync(requestId, request, services);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        _loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.Contains("Sending command:")), 
            It.IsAny<object[]>()), Times.AtLeastOnce);
        _loggerMock.Verify(
            x => x.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestId), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_LogsWarningAndReturnsBadRequest_WhenRequestIdIsEmpty()
    {
        // Arrange
        var requestId = Guid.Empty;
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

        var services = new
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object
        };

        // Act
        var result = await CreateOrderAsync(requestId, request, services);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequestResult.Value);
        _loggerMock.Verify(
            x => x.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
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

        var services = new
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default)).ReturnsAsync(false);

        // Act
        var result = await CreateOrderAsync(requestId, request, services);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        _loggerMock.Verify(
            x => x.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId), Times.Once);
    }
}
