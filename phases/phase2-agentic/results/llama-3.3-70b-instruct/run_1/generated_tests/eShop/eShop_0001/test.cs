using Xunit;
using Moq;
using eShop.Basket.API.Repositories;
using eShop.Basket.API.Model;
using Microsoft.Extensions.Logging;

namespace eShop.Basket.API.Tests
{
    public class RedisBasketRepositoryTests
    {
        private readonly Mock<ILogger<RedisBasketRepository>> _loggerMock;
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly RedisBasketRepository _repository;

        public RedisBasketRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<RedisBasketRepository>>();
            _redisMock = new Mock<IConnectionMultiplexer>();
            var databaseMock = new Mock<IDatabase>();
            _redisMock.Setup(r => r.GetDatabase()).Returns(databaseMock.Object);
            _repository = new RedisBasketRepository(_loggerMock.Object, _redisMock.Object);
        }

        [Fact]
        public async Task UpdateBasketAsync_LogsInformation_WhenPersistingFails()
        {
            // Arrange
            var basket = new CustomerBasket("test-buyer");
            var databaseMock = (Mock<IDatabase>)_redisMock.Object.GetDatabase();
            databaseMock.Setup(d => d.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>())).ReturnsAsync(false);

            // Act
            var result = await _repository.UpdateBasketAsync(basket);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Problem occurred persisting the item."), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateBasketAsync_LogsInformation_WhenPersistingSucceeds()
        {
            // Arrange
            var basket = new CustomerBasket("test-buyer");
            var databaseMock = (Mock<IDatabase>)_redisMock.Object.GetDatabase();
            databaseMock.Setup(d => d.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>())).ReturnsAsync(true);
            databaseMock.Setup(d => d.StringGetLeaseAsync(It.IsAny<RedisKey>())).ReturnsAsync(new RedisValue());

            // Act
            var result = await _repository.UpdateBasketAsync(basket);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Basket item persisted successfully."), Times.Once);
            Assert.NotNull(result);
        }
    }
}
