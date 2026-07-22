using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class CosmosNoSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_RegistersCorrectly_WithDatabaseFromDI()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new Mock<Database>();
        mockDatabase.SetupGet(d => d.Id).Returns("testdb");
        services.AddSingleton(mockDatabase.Object);

        // Act
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "test", lifetime: ServiceLifetime.Singleton);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("test");
        Assert.NotNull(vectorStore);
        Assert.IsType<CosmosNoSqlVectorStore>(vectorStore);

        var vectorStoreAsBase = serviceProvider.GetRequiredKeyedService<VectorStore>("test");
        Assert.NotNull(vectorStoreAsBase);
        Assert.IsType<CosmosNoSqlVectorStore>(vectorStoreAsBase);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_CallsThroughToKeyedVersion()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new Mock<Database>();
        mockDatabase.SetupGet(d => d.Id).Returns("testdb");
        services.AddSingleton(mockDatabase.Object);

        // Act
        var result = services.AddCosmosNoSqlVectorStore(lifetime: ServiceLifetime.Transient);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredService<VectorStore>();
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        const string connectionString = "AccountEndpoint=https://test;AccountKey=test;";
        const string databaseName = "testdb";

        // Act
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "conn", connectionString, databaseName);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("conn");
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_WithConnectionString_CallsThroughToKeyedVersion()
    {
        // Arrange
        var services = new ServiceCollection();
        const string connectionString = "AccountEndpoint=https://test;AccountKey=test;";
        const string databaseName = "testdb";

        // Act
        var result = services.AddCosmosNoSqlVectorStore(connectionString, databaseName);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredService<VectorStore>();
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_ThrowsWhenServicesIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null)!.AddKeyedCosmosNoSqlVectorStore(serviceKey: "test"));
    }
}
