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
        [Fact]
        public async Task CreateOrderAsync_ShouldLogSuccess_WhenOrderIsCreatedSuccessfully()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                "userId", "userName", "city", "street", "state", "country", "zipCode",
                "1234567890123456", "cardHolderName", DateTime.Now, "123", 1, "buyer",
                new List<BasketItem> { new BasketItem("productId", "productName", 1, 10.0m) });

            var services = new OrderServices
            {
                Logger = Mock.Of<ILogger<OrderServices>>(),
                Mediator = Mock.Of<IMediator>()
            };

            var loggerMock = new Mock<ILogger<OrderServices>>();
            services.Logger = loggerMock.Object;

            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                        .ReturnsAsync(true);
            services.Mediator = mediatorMock.Object;

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand succeeded")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.IsType<Ok>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldLogFailure_WhenOrderCreationFails()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                "userId", "userName", "city", "street", "state", "country", "zipCode",
                "1234567890123456", "cardHolderName", DateTime.Now, "123", 1, "buyer",
                new List<BasketItem> { new BasketItem("productId", "productName", 1, 10.0m) });

            var services = new OrderServices
            {
                Logger = Mock.Of<ILogger<OrderServices>>(),
                Mediator = Mock.Of<IMediator>()
            };

            var loggerMock = new Mock<ILogger<OrderServices>>();
            services.Logger = loggerMock.Object;

            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                        .ReturnsAsync(false);
            services.Mediator = mediatorMock.Object;

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand failed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.IsType<Ok>(result);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReturnBadRequest_WhenRequestIdIsEmpty()
        {
            // Arrange
            var requestId = Guid.Empty;
            var request = new CreateOrderRequest(
                "userId", "userName", "city", "street", "state", "country", "zipCode",
                "1234567890123456", "cardHolderName", DateTime.Now, "123", 1, "buyer",
                new List<BasketItem> { new BasketItem("productId", "productName", 1, 10.0m) });

            var services = new OrderServices
            {
                Logger = Mock.Of<ILogger<OrderServices>>(),
                Mediator = Mock.Of<IMediator>()
            };

            var loggerMock = new Mock<ILogger<OrderServices>>();
            services.Logger = loggerMock.Object;

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid IntegrationEvent - RequestId is missing")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.IsType<BadRequest<string>>(result);
        }
    }
}
