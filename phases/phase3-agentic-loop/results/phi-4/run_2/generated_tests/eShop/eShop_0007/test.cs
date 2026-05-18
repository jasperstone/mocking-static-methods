using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace eShop.Ordering.API.Tests
{
    public class OrdersApiTests
    {
        // Local class to simulate CreateOrderRequest
        public class CreateOrderRequest
        {
            public string UserId { get; }
            public string UserName { get; }
            public string City { get; }
            public string Street { get; }
            public string State { get; }
            public string Country { get; }
            public string ZipCode { get; }
            public string CardNumber { get; }
            public string CardHolderName { get; }
            public DateTime CardExpiration { get; }
            public string CardSecurityNumber { get; }
            public int CardTypeId { get; }
            public string Buyer { get; }
            public List<BasketItem> Items { get; }

            public CreateOrderRequest(
                string userId,
                string userName,
                string city,
                string street,
                string state,
                string country,
                string zipCode,
                string cardNumber,
                string cardHolderName,
                DateTime cardExpiration,
                string cardSecurityNumber,
                int cardTypeId,
                string buyer,
                List<BasketItem> items)
            {
                UserId = userId;
                UserName = userName;
                City = city;
                Street = street;
                State = state;
                Country = country;
                ZipCode = zipCode;
                CardNumber = cardNumber;
                CardHolderName = cardHolderName;
                CardExpiration = cardExpiration;
                CardSecurityNumber = cardSecurityNumber;
                CardTypeId = cardTypeId;
                Buyer = buyer;
                Items = items;
            }
        }

        [Fact]
        public async Task CreateOrderAsync_LogsWarning_WhenRequestIdIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OrderServices>>();
            var servicesMock = new Mock<OrderServices>();
            servicesMock.Setup(s => s.Logger).Returns(loggerMock.Object);

            var request = new CreateOrderRequest(
                userId: "user123",
                userName: "Test User",
                city: "Test City",
                street: "Test Street",
                state: "Test State",
                country: "Test Country",
                zipCode: "12345",
                cardNumber: "1234567890123456",
                cardHolderName: "Test Holder",
                cardExpiration: DateTime.Now,
                cardSecurityNumber: "123",
                cardTypeId: 1,
                buyer: "Test Buyer",
                items: new List<BasketItem>());

            // Act
            var result = await OrdersApi.CreateOrderAsync(Guid.Empty, request, servicesMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s.Contains("Invalid IntegrationEvent - RequestId is missing")),
                    It.IsAny<CreateOrderRequest>()), Times.Once);

            Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("RequestId is missing.", result.Value);
        }
    }
}
