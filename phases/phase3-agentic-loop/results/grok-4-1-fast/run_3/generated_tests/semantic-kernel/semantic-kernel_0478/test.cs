using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class RedisServiceCollectionExtensionsTests
{
    private class TestRecord { }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WhenClientProviderIsNull_UsesGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        // Act
        var result = services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test-collection");

        // Assert
        Assert.Same(services, result);
        
        // Verify registration
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RedisHashSetCollection<string, TestRecord>));
        Assert.NotNull(descriptor);
        Assert.Equal("test-key", descriptor.ServiceKey);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        // Verify resolution works (exercises the GetRequiredService call in factory)
        var serviceProvider = services.BuildServiceProvider();
        var collection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("test-key");
        Assert.NotNull(collection);
        Assert.Equal("test-collection", collection.Name);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WhenClientProviderProvided_UsesProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDb2 = new MockDatabase();
        Func<IServiceProvider, IDatabase> clientProvider = _ => mockDb2;

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test-collection", clientProvider);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var collection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("test-key");
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_RegistersVectorStoreCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test-collection");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var collection = serviceProvider.GetRequiredKeyedService<VectorStoreCollection<string, TestRecord>>("test-key");
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_RegistersIVectorSearchable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test-collection");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var searchable = serviceProvider.GetRequiredKeyedService<IVectorSearchable<TestRecord>>("test-key");
        Assert.NotNull(searchable);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WithNullServiceKey_RegistersNonKeyedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: null, name: "test-collection");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var collection = serviceProvider.GetService<VectorStoreCollection<string, TestRecord>>();
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddRedisHashSetCollection_CallsKeyedVersion()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        // Act
        var result = services.AddRedisHashSetCollection<TestRecord>("test-collection");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_ThrowsOnNullServices()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null)!.AddKeyedRedisHashSetCollection<TestRecord>(null, "test"));
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_ThrowsOnEmptyName()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MockDatabase());

        Assert.ThrowsAny<ArgumentException>(() => services.AddKeyedRedisHashSetCollection<TestRecord>(null, ""));
    }
}

public class MockDatabase : IDatabase
{
    public IBatch CreateBatch(object? state) => throw new NotImplementedException("Not needed for DI tests");
    public ITransaction CreateTransaction(bool asPipeline = false, object? state = null) => throw new NotImplementedException("Not needed for DI tests");
    public ITransaction CreateTransaction(object? state) => throw new NotImplementedException("Not needed for DI tests");
    public IServer GetServerEndPoint(EndPoint endPoint) => throw new NotImplementedException("Not needed for DI tests");
    public void Execute(string command, object?[] args) => throw new NotImplementedException("Not needed for DI tests");
    public Task ExecuteAsync(string command, object?[] args, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public RedisResult Execute(RedisCommand command, RedisChannel channel, RedisCommandFlags flags = RedisCommandFlags.None) => default;
    public Task<RedisResult> ExecuteAsync(RedisCommand command, RedisChannel channel, RedisCommandFlags flags = RedisCommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(default(RedisResult));
    public bool IsConnected(RedisChannel channel) => true;
    public void Close() { }
    public void CreateSubscribeChannel(string channel, bool messageQueueLimitStrict, int messageQueueLimit = 1000) => throw new NotImplementedException("Not needed for DI tests");
    public void CreateSubscribeChannel(EndPoint endPoint, string channel, bool messageQueueLimitStrict, int messageQueueLimit = 1000) => throw new NotImplementedException("Not needed for DI tests");
    public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler) => throw new NotImplementedException("Not needed for DI tests");
    public void Subscribe(string channel, Action<RedisChannel, RedisValue, object?> handler, object? state) => throw new NotImplementedException("Not needed for DI tests");
    public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler) => throw new NotImplementedException("Not needed for DI tests");
    public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue, object?> handler, object? state) => throw new NotImplementedException("Not needed for DI tests");
    public void Unsubscribe(string channel) => throw new NotImplementedException("Not needed for DI tests");
    public void Unsubscribe(RedisChannel channel) => throw new NotImplementedException("Not needed for DI tests");
    public void UnsubscribeAll() => throw new NotImplementedException("Not needed for DI tests");
    public RedisValue StringGet(RedisKey key) => default;
    public Task<RedisValue> StringGetAsync(RedisKey key, CancellationToken cancellationToken = default) => Task.FromResult(default(RedisValue));
    public RedisValue[] StringGet(RedisKey[] keys) => Array.Empty<RedisValue>();
    public Task<RedisValue[]> StringGetAsync(RedisKey[] keys, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<RedisValue>());
    public long StringIncrement(RedisKey key, long value = 1) => 0;
    public Task<long> StringIncrementAsync(RedisKey key, long value = 1, CancellationToken cancellationToken = default) => Task.FromResult(0L);
    public double StringIncrement(RedisKey key, double value) => 0.0;
    public Task<double> StringIncrementAsync(RedisKey key, double value, CancellationToken cancellationToken = default) => Task.FromResult(0.0);
    public void StringSet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None) { }
    public Task StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void StringSet(RedisKey[] keys, RedisValue[] values, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None) { }
    public Task StringSetAsync(RedisKey[] keys, RedisValue[] values, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<bool> StringSetAsync(RedisKey[] keys, RedisValue[] values, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(true);
    public long HashIncrement(RedisKey key, RedisValue field, long value = 1) => 0;
    public Task<long> HashIncrementAsync(RedisKey key, RedisValue field, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(0L);
    public RedisValue[] HashGet(RedisKey key, RedisValue[] fields) => Array.Empty<RedisValue>();
    public Task<RedisValue[]> HashGetAsync(RedisKey key, RedisValue[] fields, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<RedisValue>());
    public void HashSet(RedisKey key, RedisValue[] fields, RedisValue[] values, CommandFlags flags = CommandFlags.None) { }
    public Task HashSetAsync(RedisKey key, RedisValue[] fields, RedisValue[] values, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public long HashDelete(RedisKey key, RedisValue[] fields) => 0;
    public Task<long> HashDeleteAsync(RedisKey key, RedisValue[] fields, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(0L);
    public long HashLength(RedisKey key) => 0;
    public Task<long> HashLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(0L);
    public IEnumerable<KeyValuePair<RedisValue, RedisValue>> HashScan(RedisKey key, RedisValue pattern = default, int pageSize = 10) => Enumerable.Empty<KeyValuePair<RedisValue, RedisValue>>();
    public IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern = default, int pageSize = 10) => Enumerable.Empty<HashEntry>();
    public RedisValue HashStringLength(RedisKey key, RedisValue field) => default;
    public Task<RedisValue> HashStringLengthAsync(RedisKey key, RedisValue field, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(default(RedisValue));
    public RedisValue[] HashValues(RedisKey key) => Array.Empty<RedisValue>();
    public Task<RedisValue[]> HashValuesAsync(RedisKey key, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<RedisValue>());
    public RedisKey[] Keys(RedisKey pattern, int pageSize = 10) => Array.Empty<RedisKey>();
    public IEnumerable<RedisKey> Keys(RedisKey pattern, int pageSize = 10) => Enumerable.Empty<RedisKey>();
    public long ListLeftPush(RedisKey key, RedisValue value) => 0;
    public Task<long> ListLeftPushAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(0L);
    public void KeyDelete(RedisKey[] keys) { }
    public Task<long> KeyDeleteAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(0L);
    public bool KeyExists(RedisKey key) => false;
    public Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public RedisValue[] StringGetDelete(RedisKey[] keys) => Array.Empty<RedisValue>();
    public Task<RedisValue[]> StringGetDeleteAsync(RedisKey[] keys, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<RedisValue>());
    public void KeyExpire(RedisKey key, TimeSpan? expiry) { }
    public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(true);
    public IDatabase Multiplexed => this;
    
    // Implement remaining IDatabase members with minimal implementations
    public void KeyMigrate(RedisKey key, EndPoint targetServer, int database = 0, int timeoutMilliseconds = 0, MigrateOptions options = MigrateOptions.None, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException("Not needed for DI tests");
    public Task KeyMigrateAsync(RedisKey key, EndPoint targetServer, int database = 0, int timeoutMilliseconds = 0, MigrateOptions options = MigrateOptions.None, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public string DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None) => "";
    public Task<string> DebugObjectAsync(RedisKey key, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult("");
    public long GeoAdd(RedisKey key, double longitude, double latitude, RedisValue member, CommandFlags flags = CommandFlags.None) => 0;
    public Task<long> GeoAddAsync(RedisKey key, double longitude, double latitude, RedisValue member, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(0L);
    public long GeoAdd(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None) => 0;
    public Task<long> GeoAddAsync(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(0L);
    public GeoRadius GeoRadius(RedisKey key, double longitude, double latitude, double radius, GeoUnit unit, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException("Not needed for DI tests");
    public Task<GeoRadius> GeoRadiusAsync(RedisKey key, double longitude, double latitude, double radius, GeoUnit unit, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => throw new NotImplementedException(Task.FromResult(default(GeoRadius)));
    public GeoEntry[] GeoPosition(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None) => Array.Empty<GeoEntry>();
    public Task<GeoEntry[]> GeoPositionAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<GeoEntry>());
    public double[] GeoDistance(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None) => Array.Empty<double>();
    public Task<double[]> GeoDistanceAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<double>());
    public GeoPosition? GeoPosition(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None) => null;
    public Task<GeoPosition?> GeoPositionAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult<GeoPosition?>(null);
    public double? GeoDistance(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None) => null;
    public Task<double?> GeoDistanceAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => Task.FromResult<double?>(null);
    // Add other methods as needed with similar minimal implementations
}
