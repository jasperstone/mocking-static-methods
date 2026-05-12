using System.Text.Json;
using eShop.Basket.API.Model;
using eShop.Basket.API.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace eShop.Basket.API.Tests.Repositories
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
        public async Task UpdateBasketAsync_ShouldLogInformation_WhenPersistingFails()
        {
            // Arrange
            var basket = new CustomerBasket("1");
            _databaseMock.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<byte[]>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.UpdateBasketAsync(basket);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Problem occurred persisting the item."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateBasketAsync_ShouldLogInformation_WhenPersistingSucceeds()
        {
            // Arrange
            var basket = new CustomerBasket("1");
            _databaseMock.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<byte[]>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);
            _databaseMock.Setup(db => db.StringGetLeaseAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(new RedisValue(JsonSerializer.SerializeToUtf8Bytes(basket, BasketSerializationContext.Default.CustomerBasket)));

            // Act
            var result = await _repository.UpdateBasketAsync(basket);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Basket item persisted successfully."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
            Assert.NotNull(result);
        }
    }
}
