using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class RedisServiceCollectionExtensionsTests
{
    private class TestRecord { }

    private static readonly ServiceDescriptor[] ExpectedTypes = {
        typeof(RedisHashSetCollection<string, TestRecord>),
        typeof(VectorStoreCollection<string, TestRecord>),
        typeof(IVectorSearchable<TestRecord>)
    };

    [Fact]
    public void AddKeyedRedisHashSetCollection_RegistersCorrectDescriptors_WithServiceKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test");

        // Assert
        var descriptors = services.Where(d => ExpectedTypes.Contains(d.ServiceType)).ToArray();
        Assert.Equal(3, descriptors.Length);

        var hashSetDesc = descriptors.First(d => d.ServiceType == typeof(RedisHashSetCollection<string, TestRecord>));
        Assert.Equal("test-key", hashSetDesc.ServiceKey);
        Assert.Equal(ServiceLifetime.Singleton, hashSetDesc.Lifetime);

        var vectorDesc = descriptors.First(d => d.ServiceType == typeof(VectorStoreCollection<string, TestRecord>));
        Assert.Equal("test-key", vectorDesc.ServiceKey);

        var searchDesc = descriptors.First(d => d.ServiceType == typeof(IVectorSearchable<TestRecord>));
        Assert.Equal("test-key", searchDesc.ServiceKey);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_UsesGetRequiredService_WhenClientProviderNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var database = new MockDatabase();
        services.AddSingleton<IDatabase>(database);

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: null, name: "test");

        // Assert - Resolution triggers GetRequiredService call in factory
        var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false });
        var collection = sp.GetRequiredService<RedisHashSetCollection<string, TestRecord>>();
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_UsesClientProvider_WhenProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedDb = new MockDatabase();

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(
            serviceKey: null,
            name: "test",
            clientProvider: _ => expectedDb);

        // Assert - Factory uses clientProvider path
        var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false });
        var collection = sp.GetRequiredService<RedisHashSetCollection<string, TestRecord>>();
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddRedisHashSetCollection_CallsKeyedOverload_WithNullKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        // Act
        var result = services.AddRedisHashSetCollection<TestRecord>("test");

        // Assert
        Assert.Same(services, result);
        var desc = services.FirstOrDefault(d => d.ServiceType == typeof(RedisHashSetCollection<string, TestRecord>));
        Assert.NotNull(desc);
        Assert.Null(desc?.ServiceKey);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_ThrowsArgumentNullException_WhenServicesNull()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(() => services!.AddKeyedRedisHashSetCollection<TestRecord>(null!, "test"));
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_ThrowsArgumentException_WhenNameEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => services.AddKeyedRedisHashSetCollection<TestRecord>(null!, ""));
    }

    private sealed class MockDatabase : IDatabase
    {
        public IBatch CreateBatch(object? asyncState = null) => throw new NotImplementedException();
        public ITransaction CreateTransaction(object? asyncState = null) => throw new NotImplementedException();
        public void KeyMigrate(RedisKey key, EndPoint to, int database = 0, int timeoutMilliseconds = 0, MigrateOptions? options = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public object? DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void GeoAdd(RedisKey key, double longitude, double latitude, RedisValue member, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void GeoAdd(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public double? GeoDistance(RedisKey key, RedisValue member, string unit = "m", CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public IEnumerable<GeoPosition>? GeoPositions(RedisKey key, RedisValue member, string unit = "m", CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public IEnumerable<GeoPosition>? GeoPositions(RedisKey key, RedisValue[] members, string unit = "m", CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public GeoRadiusResult[] GeoRadius(RedisKey key, double longitude, double latitude, double radius, string unit = "m", int count = -1, bool includeDistances = false, bool includeHash = false, GeoUnit? geoUnit = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public GeoRadiusResult[] GeoRadius(RedisKey key, RedisValue member, double radius, string unit = "m", int count = -1, bool includeDistances = false, bool includeHash = false, GeoUnit? geoUnit = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void Execute(string command, object? parameters = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        // Add other required members as needed - minimal implementation for DI resolution
        public RedisResult Execute(string command, object? parameters = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void StringSet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public RedisValue StringGet(RedisKey key, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        // Continue with minimal stubs for all IDatabase members...
    }
}
