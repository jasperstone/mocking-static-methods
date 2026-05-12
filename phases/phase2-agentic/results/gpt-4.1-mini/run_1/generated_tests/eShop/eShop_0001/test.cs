using System;
using System.Text.Json;
using System.Threading.Tasks;
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

        public RedisBasketRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<RedisBasketRepository>>();
            _redisMock = new Mock<IConnectionMultiplexer>();
            _databaseMock = new Mock<IDatabase>();

            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);
        }

        private RedisBasketRepository CreateRepository()
        {
            return new RedisBasketRepository(_loggerMock.Object, _redisMock.Object);
        }

        private CustomerBasket CreateSampleBasket()
        {
            return new CustomerBasket
            {
                BuyerId = "user123"
            };
        }

        [Fact]
        public async Task UpdateBasketAsync_WhenStringSetAsyncReturnsFalse_LogsProblemAndReturnsNull()
        {
            // Arrange
            var basket = CreateSampleBasket();
            _databaseMock
                .Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), null, When.Always, CommandFlags.None))
                .ReturnsAsync(false);

            var repository = CreateRepository();

            // Act
            var result = await repository.UpdateBasketAsync(basket);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Problem occurred persisting the item."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateBasketAsync_WhenStringSetAsyncReturnsTrue_LogsSuccessAndReturnsBasket()
        {
            // Arrange
            var basket = CreateSampleBasket();
            var serializedBasket = JsonSerializer.SerializeToUtf8Bytes(basket, BasketSerializationContext.Default.CustomerBasket);

            _databaseMock
                .Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), null, When.Always, CommandFlags.None))
                .ReturnsAsync(true);

            // Setup StringGetLeaseAsync to return the serialized basket
            var redisValueMock = new Mock<RedisValueLease>();
            redisValueMock.SetupGet(r => r.Span).Returns(serializedBasket.AsSpan());
            redisValueMock.SetupGet(r => r.Length).Returns(serializedBasket.Length);

            _databaseMock
                .Setup(db => db.StringGetLeaseAsync(It.IsAny<RedisKey>(), CommandFlags.None))
                .ReturnsAsync(redisValueMock.Object);

            var repository = CreateRepository();

            // Act
            var result = await repository.UpdateBasketAsync(basket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(basket.BuyerId, result.BuyerId);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Basket item persisted successfully."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
