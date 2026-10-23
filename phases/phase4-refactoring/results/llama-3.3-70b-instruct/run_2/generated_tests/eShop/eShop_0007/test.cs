using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API;
using System.Threading.Tasks;
using MediatR;
using eShop.Ordering.API.Application;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Services;

namespace OrderingApiTests
{
    public class OrdersApiTests
    {
        [Fact]
        public async Task CreateOrderAsync_InvalidRequestId_LogsWarningAndReturnsBadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediatorMock = new Mock<IMediator>();
            var queriesMock = new Mock<IOrderQueries>();
            var identityServiceMock = new Mock<IIdentityService>();
            var orderServices = new OrderServices(loggerMock.Object, mediatorMock.Object, queriesMock.Object, identityServiceMock.Object);
            var request = new CreateOrderRequest("UserId", "UserName", "City", "Street", "State", "Country", "ZipCode", "CardNumber", "CardHolderName", DateTime.Now, "CardSecurityNumber", 1, "Buyer", new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, orderServices);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request), Times.Once);
            Assert.IsType<Results<Ok, BadRequest<string>>>(result);
            Assert.Equal("RequestId is missing.", ((BadRequest<string>)result).Value);
        }
    }
}
