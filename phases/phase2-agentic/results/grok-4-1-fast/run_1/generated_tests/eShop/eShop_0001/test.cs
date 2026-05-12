using System.Text.Json;
using eShop.Basket.API.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace eShop.Basket.API.Tests.Repositories;

public class RedisBasketRepositoryTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<ILogger<RedisBasketRepository>> _mockLogger;
    private readonly RedisBasketRepository _repository;

    public RedisBasketRepositoryTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockDatabase = new Mock<IDatabase>();
        _mockRedis.Setup(r => r.GetDatabase()).Returns(_mockDatabase.Object);
        _mockLogger = new Mock<ILogger<RedisBasketRepository>>();

        _repository = new RedisBasketRepository(_mockLogger.Object, _mockRedis.Object);
    }

    [Fact]
    public async Task UpdateBasketAsync_StringSetFails_LogsProblemMessage()
    {
        // Arrange
        var basket = new CustomerBasket { BuyerId = "test-user" };
        _mockDatabase.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), 
            It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _repository.UpdateBasketAsync(basket);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Problem occurred persisting the item.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateBasketAsync_StringSetSucceeds_LogsSuccessMessage()
    {
        // Arrange
        var basket = new CustomerBasket { BuyerId = "test-user" };
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(basket, BasketSerializationContext.Default.CustomerBasket);
        _mockDatabase.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), 
            It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _mockDatabase.Setup(db => db.StringGetLeaseAsync(It.IsAny<RedisKey>()))
            .ReturnsAsync(new RedisResult(jsonBytes, 1000));

        // Act
        var result = await _repository.UpdateBasketAsync(basket);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Basket item persisted successfully.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.NotNull(result);
    }
}

// Minimal CustomerBasket for testing - adjust properties as needed based on actual model
public class CustomerBasket
{
    public string BuyerId { get; set; } = string.Empty;
}
