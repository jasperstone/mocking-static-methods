using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using src.Ordering.API.Apis;
using Microsoft.AspNetCore.Http.HttpResults;
using static src.Ordering.API.Apis.OrdersApi;

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
    public async Task CreateOrderAsync_RequestIdIsEmpty_LogsWarningAndReturnsBadRequest()
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

        var services = new OrderServices
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object,
            Queries = _queriesMock.Object,
            IdentityService = _identityServiceMock.Object
        };

        // Act
        var result = await CreateOrderAsync(Guid.Empty, request, services);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("RequestId is missing.", badRequestResult.Value);
        _loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s.Contains("Invalid IntegrationEvent")), It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_LogsInformationAndReturnsOk()
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

        var mediatorResult = true;
        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
            .ReturnsAsync(mediatorResult);

        var services = new OrderServices
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object,
            Queries = _queriesMock.Object,
            IdentityService = _identityServiceMock.Object
        };

        // Act
        var result = await CreateOrderAsync(requestId, request, services);

        // Assert
        var okResult = Assert.IsType<Ok>(result);
        _loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.Contains("Sending command")), It.IsAny<object[]>()),
            Times.AtLeastOnce);
        _loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.Contains("CreateOrderCommand succeeded")), It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_MediatorReturnsFalse_LogsWarning()
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

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
            .ReturnsAsync(false);

        var services = new OrderServices
        {
            Logger = _loggerMock.Object,
            Mediator = _mediatorMock.Object,
            Queries = _queriesMock.Object,
            IdentityService = _identityServiceMock.Object
        };

        // Act
        var result = await CreateOrderAsync(requestId, request, services);

        // Assert
        var okResult = Assert.IsType<Ok>(result);
        _loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s.Contains("CreateOrderCommand failed")), It.IsAny<object[]>()),
            Times.Once);
    }
}
