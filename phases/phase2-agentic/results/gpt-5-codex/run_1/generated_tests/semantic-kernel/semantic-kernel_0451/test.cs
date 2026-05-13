using System;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.CosmosNoSql;

public sealed class CosmosNoSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_ThrowsWhenDatabaseIsMissing()
    {
        var services = new ServiceCollection();
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null, options: null, lifetime: ServiceLifetime.Singleton);

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<CosmosNoSqlVectorStore>());
        Assert.Contains(typeof(Database).FullName!, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_ResolvesVectorStoreWhenDatabaseRegistered()
    {
        using var cosmosClient = new CosmosClient(
            accountEndpoint: "https://localhost:8081",
            authKeyOrResourceToken: "C2F+dummypassword==",
            new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });

        var database = cosmosClient.GetDatabase("test-db");

        var services = new ServiceCollection();
        services.AddSingleton(database);
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null, options: null, lifetime: ServiceLifetime.Singleton);

        using var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<CosmosNoSqlVectorStore>();
        var vectorStore = provider.GetRequiredService<VectorStore>();

        Assert.Same(store, vectorStore);
    }
}
