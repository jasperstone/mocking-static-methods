using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class CosmosNoSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCosmosNoSqlVectorStoreRegistersVectorStore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Azure.Cosmos.Database>(new Microsoft.Azure.Cosmos.Database());

        // Act
        services.AddCosmosNoSqlVectorStore();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetService<VectorStore>();
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStoreRegistersCosmosNoSqlVectorStore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Azure.Cosmos.Database>(new Microsoft.Azure.Cosmos.Database());

        // Act
        services.AddCosmosNoSqlVectorStore();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var cosmosNoSqlVectorStore = serviceProvider.GetService<CosmosNoSqlVectorStore>();
        Assert.NotNull(cosmosNoSqlVectorStore);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStoreRegistersVectorStore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Azure.Cosmos.Database>(new Microsoft.Azure.Cosmos.Database());

        // Act
        services.AddKeyedCosmosNoSqlVectorStore("key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetService<VectorStore>("key");
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStoreRegistersCosmosNoSqlVectorStore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Azure.Cosmos.Database>(new Microsoft.Azure.Cosmos.Database());

        // Act
        services.AddKeyedCosmosNoSqlVectorStore("key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var cosmosNoSqlVectorStore = serviceProvider.GetService<CosmosNoSqlVectorStore>("key");
        Assert.NotNull(cosmosNoSqlVectorStore);
    }
}
