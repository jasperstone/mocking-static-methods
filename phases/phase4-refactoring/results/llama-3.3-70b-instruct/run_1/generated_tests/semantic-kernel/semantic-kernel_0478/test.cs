using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Microsoft.Extensions.VectorData;

public class RedisServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedRedisHashSetCollection_WithClientProvider_ServiceCollectionIsUpdated()
    {
        // Arrange
        var services = new ServiceCollection();
        var clientProvider = new Func<IServiceProvider, IDatabase>(sp => new Mock<IDatabase>().Object);

        // Act
        services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name: "test", clientProvider: clientProvider);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var redisHashSetCollection = serviceProvider.GetService(typeof(RedisHashSetCollection<string, string>));
        Assert.NotNull(redisHashSetCollection);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WithoutClientProvider_ServiceCollectionIsUpdated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(options =>
        {
            options.InstanceName = "test";
            options.Configuration = "localhost";
        });

        // Act
        services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name: "test");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var redisHashSetCollection = serviceProvider.GetService(typeof(RedisHashSetCollection<string, string>));
        Assert.NotNull(redisHashSetCollection);
    }
}
