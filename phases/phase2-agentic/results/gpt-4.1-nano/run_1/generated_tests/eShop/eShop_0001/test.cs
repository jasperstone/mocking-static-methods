using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using eShop.Basket.API.Repositories;
using eShop.Basket.API.Model;

namespace eShop.Basket.Tests
{
    public class RedisBasketRepositoryTests
    {
        private readonly Mock<ILogger<RedisBasketRepository>> _loggerMock;
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly Mock<IDatabase> _databaseMock;
        private readonly RedisBasketRepository _repository;

        public RedisBasketRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<RedisBasketRepository>>();
            _redisMock = new Mock<IConnectionMultiplexer>();
            _databaseMock = new Mock<IDatabase>();
            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);
            _repository = new RedisBasketRepository(_loggerMock.Object, _redisMock.Object);
        }

        [Fact]
        public async Task UpdateBasketAsync_ShouldLogInformation_WhenStringSetFails()
        {
            // Arrange
            var basket = new CustomerBasket { BuyerId = "user123" };
            var jsonBytes = new byte[] { 1, 2, 3 };
            _databaseMock.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), null, When.Always, CommandFlags.None))
                         .ReturnsAsync(false);
            _databaseMock.Setup(db => db.StringGetLeaseAsync(It.IsAny<RedisKey>()))
                         .ReturnsAsync(new RedisValue<byte[]>(jsonBytes));

            // Act
            var result = await _repository.UpdateBasketAsync(basket);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Problem occurred persisting the item.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
