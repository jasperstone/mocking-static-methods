using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class CosmosNoSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithDatabaseFromDI_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var database = new MockDatabase();
        services.AddSingleton<Database>(database);

        // Act
        var result = services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "test", lifetime: ServiceLifetime.Transient);

        // Assert registration
        Assert.Same(result, services);
        
        // Build and verify resolution works (triggers GetRequiredService)
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("test");
        Assert.NotNull(vectorStore);
        Assert.True(database.WasRetrieved);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_Unkeyed_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var database = new MockDatabase();
        services.AddSingleton<Database>(database);

        // Act
        var result = services.AddCosmosNoSqlVectorStore(lifetime: ServiceLifetime.Transient);

        // Assert
        Assert.Same(result, services);
        
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredService<VectorStore>();
        Assert.NotNull(vectorStore);
        Assert.True(database.WasRetrieved);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_MissingDatabase_ThrowsOnResolution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "test", lifetime: ServiceLifetime.Transient);

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("test"));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_DoesNotRequireDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var database = new MockDatabase();
        services.AddSingleton<Database>(database);

        // Act
        var result = services.AddKeyedCosmosNoSqlVectorStore(
            serviceKey: "test",
            connectionString: "fake-connection-string",
            databaseName: "fake-db",
            lifetime: ServiceLifetime.Transient);

        // Assert
        Assert.Same(result, services);
        
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("test");
        Assert.NotNull(vectorStore);
        Assert.False(database.WasRetrieved);
    }

    private sealed class MockDatabase : Database
    {
        internal bool WasRetrieved { get; private set; }

        public MockDatabase()
        {
            WasRetrieved = true;
        }

        public override string Id => "mock";
        public override string? DatabaseId => "mock";

        public override Task<DatabaseResponse> DeleteAsync(RequestOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public override Task<Container> GetContainerAsync(string containerId, string? containerRid = null)
            => throw new NotImplementedException();

        public override FeedIterator<dynamic> GetContainerQueryIterator(
            string queryText, 
            string? continuationToken = null, 
            QueryRequestOptions? requestOptions = null)
            => throw new NotImplementedException();

        public override Task<User> GetUserAsync(string userId, string? userRid = null)
            => throw new NotImplementedException();

        public override FeedIterator<T> GetUserQueryIterator<T>(
            QueryDefinition queryDefinition,
            string? continuationToken = null, 
            QueryRequestOptions? requestOptions = null)
            => throw new NotImplementedException();

        public override Task<DatabaseResponse> ReadAsync(RequestOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public override Task<ThroughputResponse> ReadThroughputAsync(RequestOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public override Task<ResponseMessage> ReplaceThroughputAsync(
            int throughput, 
            RequestOptions? options = null, 
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
