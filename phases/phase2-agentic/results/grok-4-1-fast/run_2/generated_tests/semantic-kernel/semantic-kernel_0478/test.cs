using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;
using Xunit;

public class RedisServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedRedisHashSetCollection_WithClientProvider_DoesNotCallGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(Mock.Of<IDatabase>());

        Func<IServiceProvider, IDatabase> clientProvider = _ => Mock.Of<IDatabase>();
        var callCount = 0;
        Func<IServiceProvider, IDatabase> trackingProvider = sp =>
        {
            callCount++;
            return clientProvider(sp);
        };

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test", "test-collection", trackingProvider);

        // Assert
        Assert.Equal(0, callCount);
        using var serviceProvider = services.BuildServiceProvider();
        var collection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("test");
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WithNullClientProvider_CallsGetRequiredService()
    {
        // Arrange
        var mockDatabase = Mock.Of<IDatabase>();
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(mockDatabase);

        // Act
        var ex = Record.Exception(() => services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test", "test-collection"));

        // Assert
        Assert.Null(ex);
        using var serviceProvider = services.BuildServiceProvider();
        var collection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("test");
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WithNullClientProvider_NoIDatabaseRegistered_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test", "test-collection"));
        Assert.Contains("IDatabase", ex.Message);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_RegistersCorrectServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(Mock.Of<IDatabase>());

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test", "test-collection");

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("test"));
        Assert.NotNull(serviceProvider.GetRequiredKeyedService<VectorStoreCollection<string, TestRecord>>("test"));
        Assert.NotNull(serviceProvider.GetRequiredKeyedService<IVectorSearchable<TestRecord>>("test"));
    }

    [Fact]
    public void AddRedisHashSetCollection_CallsThroughToKeyedMethod()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(Mock.Of<IDatabase>());

        // Act
        services.AddRedisHashSetCollection<TestRecord>("test-collection");

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<VectorStoreCollection<string, TestRecord>>());
    }

    private class TestRecord { }
}
