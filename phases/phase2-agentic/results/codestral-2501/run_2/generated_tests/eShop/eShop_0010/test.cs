using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        private readonly Mock<ILogger<OrderServices>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly OrderServices _services;

        public OrdersApiTests()
        {
            _loggerMock = new Mock<ILogger<OrderServices>>();
            _mediatorMock = new Mock<IMediator>();
            _services = new OrderServices
            {
                Logger = _loggerMock.Object,
                Mediator = _mediatorMock.Object
            };
        }

        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
        {
            // Arrange
            var requestId = Guid.Empty;
            var request = new CreateOrderRequest(
                "userId",
                "userName",
                "city",
                "street",
                "state",
                "country",
                "zipCode",
                "cardNumber",
                "cardHolderName",
                DateTime.Now,
                "cardSecurityNumber",
                1,
                "buyer",
                new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, _services);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid IntegrationEvent - RequestId is missing")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.IsType<BadRequest<string>>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequestId_LogsInformationAndReturnsOk()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                "userId",
                "userName",
                "city",
                "street",
                "state",
                "country",
                "zipCode",
                "cardNumber",
                "cardHolderName",
                DateTime.Now,
                "cardSecurityNumber",
                1,
                "buyer",
                new List<BasketItem>());

            _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, _services);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand succeeded")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.IsType<Ok>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_CommandFails_LogsWarningAndReturnsOk()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                "userId",
                "userName",
                "city",
                "street",
                "state",
                "country",
                "zipCode",
                "cardNumber",
                "cardHolderName",
                DateTime.Now,
                "cardSecurityNumber",
                1,
                "buyer",
                new List<BasketItem>());

            _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, _services);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand failed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.IsType<Ok>(result);
        }
    }
}
