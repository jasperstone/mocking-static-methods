using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection;

public class CosmosNoSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_RegistersCorrectly_WithDatabaseFromDI()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new Mock<Database>();
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
    public void AddKeyedCosmosNoSqlVectorStore_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null)!.AddKeyedCosmosNoSqlVectorStore(serviceKey: null));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_VerifiesGetRequiredServiceIsCalled()
    {
        // This test verifies the lambda factory uses sp.GetRequiredService<Database>()
        // by ensuring the registration fails when Database is not registered
        var services = new ServiceCollection();
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "test", lifetime: ServiceLifetime.Singleton);

        // Act & Assert - should throw when resolving due to missing Database
        var serviceProvider = services.BuildServiceProvider();
        Assert.ThrowsAny<InvalidOperationException>(() => 
            serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("test"));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_NullConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        const string databaseName = "testdb";

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "conn", connectionString: null!, databaseName));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_EmptyConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        const string databaseName = "testdb";

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "conn", connectionString: "", databaseName));
    }
}
