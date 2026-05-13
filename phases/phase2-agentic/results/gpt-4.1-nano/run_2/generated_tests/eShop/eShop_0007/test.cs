using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using static src.Ordering.API.Apis.OrdersApi;

namespace OrderingApi.Tests
{
    public class OrdersApiTests
    {
        private class DummyLogger : ILogger
        {
            public List<string> WarningMessages = new List<string>();
            public List<string> InfoMessages = new List<string>();

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                if (logLevel == LogLevel.Warning)
                {
                    WarningMessages.Add(message);
                }
                else if (logLevel == LogLevel.Information)
                {
                    InfoMessages.Add(message);
                }
            }
        }

        [Fact]
        public async Task CreateOrderAsync_RequestId_Empty_Should_LogWarning()
        {
            // Arrange
            var logger = new DummyLogger();
            var mediatorMock = new Mock<IMediator>();
            var services = new OrderServices
            {
                Logger = logger,
                Mediator = mediatorMock.Object,
                Queries = null,
                IdentityService = null
            };
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
                CardExpiration: DateTime.Now.AddYears(1),
                CardSecurityNumber: "123",
                CardTypeId: 1,
                Buyer: "Buyer",
                Items: new List<BasketItem>());

            // Act
            var result = await CreateOrderAsync(Guid.Empty, request, services);

            // Assert
            Assert.IsType<BadRequest<string>>(result);
            Assert.Contains("RequestId is missing", logger.WarningMessages);
        }

        [Fact]
        public async Task CreateOrderAsync_RequestId_Valid_Should_LogInformationAndWarning()
        {
            // Arrange
            var logger = new DummyLogger();
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>()))
                .ReturnsAsync(true);
            var services = new OrderServices
            {
                Logger = logger,
                Mediator = mediatorMock.Object,
                Queries = null,
                IdentityService = null
            };
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
                CardExpiration: DateTime.Now.AddYears(1),
                CardSecurityNumber: "123",
                CardTypeId: 1,
                Buyer: "Buyer",
                Items: new List<BasketItem>());

            // Act
            var result = await CreateOrderAsync(Guid.NewGuid(), request, services);

            // Assert
            Assert.IsType<Results<Ok, BadRequest<string>>>(result);
            Assert.Contains($"Sending command: {typeof(CreateOrderCommand).Name}", logger.InfoMessages[0]);
            Assert.Contains($"CreateOrderCommand succeeded - RequestId: ", logger.InfoMessages[1]);
        }
    }
}
