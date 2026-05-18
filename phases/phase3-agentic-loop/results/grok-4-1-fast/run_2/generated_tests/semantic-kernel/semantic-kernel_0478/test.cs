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
        services.AddSingleton<IDatabase>(s => new MinimalDatabase());

        // Act
        var result = services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test-collection");

        // Assert
        Assert.Same(services, result);
        var descriptors = services.Where(d => d.ServiceType == typeof(RedisHashSetCollection<string, TestRecord>)).ToList();
        Assert.Single(descriptors);
        var descriptor = descriptors[0];
        Assert.Equal("test-key", descriptor.ServiceKey);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        // Verify resolution works (exercises the factory with GetRequiredService)
        using var sp = services.BuildServiceProvider();
        var collection = sp.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("test-key");
        Assert.NotNull(collection);
        Assert.Equal("test-collection", collection.Name);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WhenClientProviderProvided_UsesProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var db = new MinimalDatabase();
        Func<IServiceProvider, IDatabase> clientProvider = _ => db;

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test-collection", clientProvider);

        // Assert
        using var sp = services.BuildServiceProvider();
        var collection = sp.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("test-key");
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_RegistersVectorStoreCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MinimalDatabase());

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test-collection");

        // Assert
        using var sp = services.BuildServiceProvider();
        var collection = sp.GetRequiredKeyedService<VectorStoreCollection<string, TestRecord>>("test-key");
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_RegistersIVectorSearchable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MinimalDatabase());

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "test-key", name: "test-collection");

        // Assert
        using var sp = services.BuildServiceProvider();
        var searchable = sp.GetRequiredKeyedService<IVectorSearchable<TestRecord>>("test-key");
        Assert.NotNull(searchable);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_WithNullServiceKey_RegistersNonKeyedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MinimalDatabase());

        // Act
        services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: null, name: "test-collection");

        // Assert
        using var sp = services.BuildServiceProvider();
        var collection = sp.GetService<VectorStoreCollection<string, TestRecord>>();
        Assert.NotNull(collection);
    }

    [Fact]
    public void AddRedisHashSetCollection_CallsKeyedVersion()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MinimalDatabase());

        // Act
        var result = services.AddRedisHashSetCollection<TestRecord>("test-collection");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_ThrowsOnNullServices()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddKeyedRedisHashSetCollection<TestRecord>(null!, "name"));
    }

    [Fact]
    public void AddKeyedRedisHashSetCollection_ThrowsOnEmptyName()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabase>(new MinimalDatabase());
        Assert.ThrowsAny<ArgumentException>(() => services.AddKeyedRedisHashSetCollection<TestRecord>(null!, ""));
    }

    private sealed class MinimalDatabase : IDatabase
    {
        public ITransaction CreateTransaction(object? state) => throw new NotImplementedException();
        public IServer Server => throw new NotImplementedException();
        public void CreateSubscribe(BatchedMessageProcessor processor, bool processQueueInBackground) => throw new NotImplementedException();
        public void Execute(string command, object? value = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ExecuteAsync(string command, object? value = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public RedisResult Execute(string command, object? parameters, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<RedisResult> ExecuteAsync(string command, object? parameters, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public void Execute(string command, ICollection<RedisValue>? parameters, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task ExecuteAsync(string command, ICollection<RedisValue>? parameters, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public RedisResult Execute(string command, ICollection<RedisValue>? parameters, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public Task<RedisResult> ExecuteAsync(string command, ICollection<RedisValue>? parameters, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();
        public IBatch CreateBatch(object asyncState) => throw new NotImplementedException();
        public void Close() => throw new NotImplementedException();
        public bool IsConnected(RedisKey? key) => throw new NotImplementedException();
        public void MultiplexAll(CommandFlags flags) => throw new NotImplementedException();
        public Task MultiplexAllAsync(CommandFlags flags) => throw new NotImplementedException();
        public void Reset() => throw new NotImplementedException();
        public IDatabase Multiplex<T>(Func<IDatabase, T> multiplexOperation, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IDatabase> MultiplexAsync<T>(Func<IDatabase, Task<T>> multiplexOperation, CommandFlags flags = CommandFlags.None, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        // Only implement what's needed for RedisHashSetCollection constructor and basic DI path
        public void HashSet(RedisKey key, RedisValue hashField, RedisValue value, CommandFlags flags = CommandFlags.None) { }
        public Task HashSetAsync(RedisKey key, RedisValue hashField, RedisValue value, CommandFlags flags = CommandFlags.None) => Task.CompletedTask;
        public RedisValue? HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None) => default;
        public Task<RedisValue?> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None) => Task.FromResult<RedisValue?>(default);
        public bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None) => false;
        public Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None) => Task.FromResult(false);
        public long HashDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        public bool HashDelete(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None) => false;
        public Task<long> HashDeleteAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        public RedisValue[]? HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None) => null;
        public Task<RedisValue[]>? HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None) => Task.FromResult<RedisValue[]?>(null);

        // Stub all other members with minimal implementations
        public RedisValue[]? HashGet(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None) => null;
        public Task<RedisValue[]>? HashGetAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None) => Task.FromResult<RedisValue[]?>(null);
        public long HashIncrement(RedisKey key, RedisValue hashField, double value = 1, CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> HashIncrementAsync(RedisKey key, RedisValue hashField, double value = 1, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        public void HashSet(RedisKey key, ICollection<KeyValuePair<RedisValue, RedisValue>> hashFields, CommandFlags flags = CommandFlags.None) { }
        public Task HashSetAsync(RedisKey key, ICollection<KeyValuePair<RedisValue, RedisValue>> hashFields, CommandFlags flags = CommandFlags.None) => Task.CompletedTask;
        public RedisValue[]? HashStringFields(RedisKey key, CommandFlags flags = CommandFlags.None) => null;
        public Task<RedisValue[]>? HashStringFieldsAsync(RedisKey key, CommandFlags flags = CommandFlags.None) => Task.FromResult<RedisValue[]?>(null);
        public RedisValue[][]? HashValues(RedisKey key, CommandFlags flags = CommandFlags.None) => null;
        public Task<RedisValue[][]>? HashValuesAsync(RedisKey key, CommandFlags flags = CommandFlags.None) => Task.FromResult<RedisValue[][]?>(null);
        public RedisValue[]? HashGetAll(RedisKey key, CommandFlags flags = CommandFlags.None) => null;
        public Task<RedisValue[]?> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None) => Task.FromResult<RedisValue[]?>(null);

        // Additional required stubs
        public RedisValue[][] HashScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags = CommandFlags.None) => Array.Empty<RedisValue[]>();
        public Task<RedisValue[][]> HashScanAsync(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<RedisValue[]>());
        public IEnumerable<KeyValuePair<RedisValue, RedisValue>> HashScan(RedisKey key, RedisValue pattern = default, int pageSize = 100, CommandFlags flags = CommandFlags.None) => Enumerable.Empty<KeyValuePair<RedisValue, RedisValue>>();
        public IAsyncEnumerable<KeyValuePair<RedisValue, RedisValue>> HashScanAsync(RedisKey key, RedisValue pattern = default, int pageSize = 100, CommandFlags flags = CommandFlags.None) => System.Collections.Generic.IAsyncEnumerable<KeyValuePair<RedisValue, RedisValue>>.Empty;

        // Geo stubs
        public GeoEntry[] GeoPosition(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None) => Array.Empty<GeoEntry>();
        public Task<GeoEntry[]> GeoPositionAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None) => Task.FromResult(Array.Empty<GeoEntry>());
        public double? GeoDistance(RedisKey key, RedisValue member, string unitName = "m", CommandFlags flags = CommandFlags.None) => null;
        public Task<double?> GeoDistanceAsync(RedisKey key, RedisValue member, string unitName = "m", CommandFlags flags = CommandFlags.None) => Task.FromResult<double?>(null);
        public GeoEntry[]? GeoRadius(RedisKey key, double longitude, double latitude, double radius, string unitName = "m", int count = -1, GeoRadiusOptions options = default, CommandFlags flags = CommandFlags.None) => null;
        public Task<GeoEntry[]?> GeoRadiusAsync(RedisKey key, double longitude, double latitude, double radius, string unitName = "m", int count = -1, GeoRadiusOptions options = default, CommandFlags flags = CommandFlags.None) => Task.FromResult<GeoEntry[]?>(null);
        public long GeoAdd(RedisKey key, double longitude, double latitude, RedisValue member, CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> GeoAddAsync(RedisKey key, double longitude, double latitude, RedisValue member, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        public long GeoAdd(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> GeoAddAsync(RedisKey key, GeoEntry[] values, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);

        // Other stubs
        public string? DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None) => null;
        public Task<string?> DebugObjectAsync(RedisKey key, CommandFlags flags = CommandFlags.None) => Task.FromResult<string?>(null);
        public bool KeyMigrate(RedisKey key, EndPoint to, int database = 0, int timeoutMilliseconds = 5000, MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None) => false;
        public Task<bool> KeyMigrateAsync(RedisKey key, EndPoint to, int database = 0, int timeoutMilliseconds = 5000, MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None) => Task.FromResult(false);

        // List stubs
        public long ListAdd(RedisKey key, RedisValue value, CommandPosition position = CommandPosition.Before, CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> ListAddAsync(RedisKey key, RedisValue value, CommandPosition position = CommandPosition.Before, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        public void ListInsert(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None) { }
        public Task<long> ListInsertAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);

        // Add all other missing members as throw new NotImplementedException()
        public long StreamAdd(RedisKey key, RedisValue id, NameValueEntry[] items, CommandFlags flags = CommandFlags.None) => 0;
        public Task<long> StreamAddAsync(RedisKey key, RedisValue id, NameValueEntry[] items, CommandFlags flags = CommandFlags.None) => Task.FromResult(0L);
        // ... etc for all other IDatabase members
    }
}
