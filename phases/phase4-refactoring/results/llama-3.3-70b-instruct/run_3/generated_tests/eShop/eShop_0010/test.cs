using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Models;
using MediatR;

namespace OrderingApiTests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediatorMock = new Mock<IMediator>();
            var queriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
            servicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            var requestId = Guid.Empty;
            var request = new CreateOrderRequest(
                "UserId",
                "UserName",
                "City",
                "Street",
                "State",
                "Country",
                "ZipCode",
                "CardNumber",
                "CardHolderName",
                DateTime.Now,
                "CardSecurityNumber",
                1,
                "Buyer",
                new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequestId_LogsInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediatorMock = new Mock<IMediator>();
            var queriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
            servicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                "UserId",
                "UserName",
                "City",
                "Street",
                "State",
                "Country",
                "ZipCode",
                "CardNumber",
                "CardHolderName",
                DateTime.Now,
                "CardSecurityNumber",
                1,
                "Buyer",
                new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_CreateOrderCommandSucceeded_LogsInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediatorMock = new Mock<IMediator>();
            var queriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
            servicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>())).ReturnsAsync(true);
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                "UserId",
                "UserName",
                "City",
                "Street",
                "State",
                "Country",
                "ZipCode",
                "CardNumber",
                "CardHolderName",
                DateTime.Now,
                "CardSecurityNumber",
                1,
                "Buyer",
                new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation("CreateOrderCommand succeeded - RequestId: {RequestId}", requestId), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_CreateOrderCommandFailed_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediatorMock = new Mock<IMediator>();
            var queriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);
            servicesMock.Setup(s => s.Mediator).Returns(mediatorMock.Object);
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>())).ReturnsAsync(false);
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                "UserId",
                "UserName",
                "City",
                "Street",
                "State",
                "Country",
                "ZipCode",
                "CardNumber",
                "CardHolderName",
                DateTime.Now,
                "CardSecurityNumber",
                1,
                "Buyer",
                new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", requestId), Times.Once);
        }
    }
}
