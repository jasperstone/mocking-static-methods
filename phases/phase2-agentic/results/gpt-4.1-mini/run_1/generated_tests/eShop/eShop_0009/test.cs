using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Apis;
using MediatR;

namespace eShop.Ordering.API.Tests.Apis
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

            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand succeeded")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
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

            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand failed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal BasketItem stub for compilation
    public class BasketItem { }

    // Minimal CreateOrderCommand stub for compilation
    public class CreateOrderCommand
    {
        public CreateOrderCommand(List<BasketItem> items, string userId, string userName, string city, string street,
            string state, string country, string zipCode, string cardNumber, string cardHolderName,
            DateTime cardExpiration, string cardSecurityNumber, int cardTypeId)
        {
        }
    }

    // Minimal IdentifiedCommand stub for compilation
    public class IdentifiedCommand<TCommand, TResult>
    {
        public IdentifiedCommand(TCommand command, Guid id)
        {
            Command = command;
            Id = id;
        }

        public TCommand Command { get; }
        public Guid Id { get; }

        public string GetGenericTypeName() => typeof(TCommand).Name;
    }

    // Minimal interfaces for compilation
    public interface IOrderQueries { }
    public interface IIdentityService { }
}
