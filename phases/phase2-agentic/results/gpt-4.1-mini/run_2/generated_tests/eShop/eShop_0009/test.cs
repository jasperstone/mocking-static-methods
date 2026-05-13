using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using eShop.Ordering.API;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_LogsInformationOnSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var mediatorMock = new Mock<IMediator>();
            var queriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();

            var services = new OrderServices(mediatorMock.Object, queriesMock.Object, identityServiceMock.Object, loggerMock.Object);

            var requestId = Guid.NewGuid();

            var items = new List<BasketItem>
            {
                new BasketItem { /* minimal properties if needed */ }
            };

            var request = new CreateOrderRequest(
                UserId: "user1",
                UserName: "User One",
                City: "City",
                Street: "Street",
                State: "State",
                Country: "Country",
                ZipCode: "12345",
                CardNumber: "1234567890123456",
                CardHolderName: "Holder",
                CardExpiration: DateTime.UtcNow.AddYears(1),
                CardSecurityNumber: "123",
                CardTypeId: 1,
                Buyer: "Buyer",
                Items: items);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            // Verify the LogInformation call on line 159 (the success log)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand succeeded - RequestId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Also verify the initial LogInformation call with command info
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending command: CreateOrderCommand")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.IsType<Results<Ok, BadRequest<string>>>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_LogsWarningOnFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var mediatorMock = new Mock<IMediator>();
            var queriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();

            var services = new OrderServices(mediatorMock.Object, queriesMock.Object, identityServiceMock.Object, loggerMock.Object);

            var requestId = Guid.NewGuid();

            var items = new List<BasketItem>();

            var request = new CreateOrderRequest(
                UserId: "user1",
                UserName: "User One",
                City: "City",
                Street: "Street",
                State: "State",
                Country: "Country",
                ZipCode: "12345",
                CardNumber: "1234567890123456",
                CardHolderName: "Holder",
                CardExpiration: DateTime.UtcNow.AddYears(1),
                CardSecurityNumber: "123",
                CardTypeId: 1,
                Buyer: "Buyer",
                Items: items);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            // Verify the LogWarning call on failure
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand failed - RequestId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.IsType<Results<Ok, BadRequest<string>>>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_ReturnsBadRequest_WhenRequestIdIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var mediatorMock = new Mock<IMediator>();
            var queriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();

            var services = new OrderServices(mediatorMock.Object, queriesMock.Object, identityServiceMock.Object, loggerMock.Object);

            var requestId = Guid.Empty;

            var request = new CreateOrderRequest(
                UserId: "user1",
                UserName: "User One",
                City: "City",
                Street: "Street",
                State: "State",
                Country: "Country",
                ZipCode: "12345",
                CardNumber: "1234567890123456",
                CardHolderName: "Holder",
                CardExpiration: DateTime.UtcNow.AddYears(1),
                CardSecurityNumber: "123",
                CardTypeId: 1,
                Buyer: "Buyer",
                Items: new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid IntegrationEvent - RequestId is missing")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.IsType<Results<BadRequest<string>, Ok>>(result);
        }
    }

    // Minimal BasketItem stub for compilation
    public class BasketItem
    {
    }

    // Minimal stubs for dependencies
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(object request, CancellationToken cancellationToken = default);
    }

    public interface IOrderQueries
    {
    }

    public interface IIdentityService
    {
        string GetUserIdentity();
    }

    public record IdentifiedCommand<TCommand, TResult>(TCommand Command, Guid Id)
    {
        public string GetGenericTypeName() => typeof(TCommand).Name;
    }

    public record CreateOrderCommand(
        List<BasketItem> Items,
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
        int CardTypeId);

    // Minimal Results type to satisfy return type
    public class Results<T1, T2> { }
    public class Results<T1, T2, T3> { }
    public class Ok { }
    public class BadRequest<T> { }
}
