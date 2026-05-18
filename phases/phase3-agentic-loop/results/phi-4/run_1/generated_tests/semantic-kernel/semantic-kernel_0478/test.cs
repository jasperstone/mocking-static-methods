using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;

public class RedisServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedRedisHashSetCollection_WhenClientProviderIsNull_UsesServiceProviderToGetDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(Mock.Of<IDatabase>());

        var serviceProvider = services.BuildServiceProvider();

        // Act
        RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<object>(services, null, "testName");

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RedisHashSetCollection<string, object>));
        Assert.NotNull(descriptor);

        var factory = (Func<IServiceProvider, RedisHashSetCollection<string, object>>)descriptor.ImplementationFactory;
        var collection = factory(serviceProvider);

        var mockDatabase = Mock.Get(serviceProvider.GetRequiredService<IDatabase>());
        mockDatabase.Verify(db => db, Times.Once);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WhenClientProviderIsProvided_UsesClientProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = new Mock<IServiceProvider>();
        var database = Mock.Of<IDatabase>();

        serviceProvider.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(database);

        // Act
        RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<object>(
            services,
            null,
            "testName",
            sp => database);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RedisHashSetCollection<string, object>));
        Assert.NotNull(descriptor);

        var factory = (Func<IServiceProvider, RedisHashSetCollection<string, object>>)descriptor.ImplementationFactory;
        var collection = factory(serviceProvider.Object);

        Assert.Same(database, collection.Database);
    }
}
