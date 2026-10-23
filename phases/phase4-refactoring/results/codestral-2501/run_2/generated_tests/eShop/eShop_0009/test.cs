using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;
using MediatR;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_ShouldLogInformation_WhenCommandSucceeds()
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
                "1234567890123456",
                "cardHolderName",
                DateTime.Now,
                "123",
                1,
                "buyer",
                new List<BasketItem>());

            var loggerMock = new Mock<ILogger<OrderServices>>();
            var mediatorMock = new Mock<IMediator>();

            mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
                .ReturnsAsync(true);

            var services = new OrderServices(
                mediatorMock.Object,
                loggerMock.Object
            );

            // Act
            var result = await OrdersApi.CreateOrderAsync(requestId, request, services);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateOrderCommand succeeded")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);

            Assert.IsType<Ok>(result);
        }
    }
}
