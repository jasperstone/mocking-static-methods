using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class CosmosNoSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithDatabaseFromDI_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Database>(new MockDatabase());

        // Act
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "test-key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("test-key");
        Assert.NotNull(vectorStore);
        var vectorStoreDirect = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("test-key");
        Assert.Same(vectorStore, vectorStoreDirect);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_CallsKeyedVersionCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Database>(new MockDatabase());

        // Act
        var result = services.AddCosmosNoSqlVectorStore();

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredService<VectorStore>() as CosmosNoSqlVectorStore;
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
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "conn-key", connectionString, databaseName);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("conn-key");
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_WithConnectionString_CallsKeyedVersionCorrectly()
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
        var vectorStore = serviceProvider.GetRequiredService<VectorStore>() as CosmosNoSqlVectorStore;
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddKeyedCosmosNoSqlVectorStore(serviceKey: null));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_NullConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        string? connectionString = null;
        const string databaseName = "testdb";

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null, connectionString!, databaseName));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithDifferentLifetimes_RespectsLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Database>(new MockDatabase());

        // Act
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "scoped-key", lifetime: ServiceLifetime.Scoped);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var scope1 = serviceProvider.CreateScope();
        var scope2 = serviceProvider.CreateScope();

        var vectorStore1 = scope1.ServiceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("scoped-key");
        var vectorStore2 = scope2.ServiceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("scoped-key");

        Assert.NotSame(vectorStore1, vectorStore2);
    }

    private sealed class MockDatabase : Database
    {
        // Minimal implementation for DI resolution testing
        public override string Id => "mock";
        public override string? DatabaseId => "mock";
    }
}
