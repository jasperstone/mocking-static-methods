using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class CosmosNoSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithDatabaseService_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new Mock<Database>();
        services.AddSingleton(mockDatabase.Object);

        // Act
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "test", lifetime: ServiceLifetime.Singleton);

        // Assert - Build succeeds without exception
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("test");
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithoutDatabaseService_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "test"));
        Assert.Contains("No service for type 'Microsoft.Azure.Cosmos.Database' has been registered", exception.Message);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithNullServiceKey_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new Mock<Database>();
        services.AddSingleton(mockDatabase.Object);

        // Act
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null, lifetime: ServiceLifetime.Singleton);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredService<CosmosNoSqlVectorStore>();
        Assert.NotNull(vectorStore);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_CallsKeyedOverload()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new Mock<Database>();
        services.AddSingleton(mockDatabase.Object);

        // Act
        services.AddCosmosNoSqlVectorStore(lifetime: ServiceLifetime.Singleton);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var vectorStore = serviceProvider.GetRequiredService<VectorStore>();
        Assert.IsType<CosmosNoSqlVectorStore>(vectorStore);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_RegistersVectorStoreInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new Mock<Database>();
        services.AddSingleton(mockDatabase.Object);

        // Act
        services.AddKeyedCosmosNoSqlVectorStore("test-key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var keyedVectorStore = serviceProvider.GetRequiredKeyedService<VectorStore>("test-key");
        Assert.IsType<CosmosNoSqlVectorStore>(keyedVectorStore);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithScopedLifetime_UsesScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new Mock<Database>();
        services.AddScoped(mockDatabase.Object);

        // Act
        services.AddKeyedCosmosNoSqlVectorStore(serviceKey: "scoped", lifetime: ServiceLifetime.Scoped);

        // Assert - No exception during registration
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var vectorStore = scope.ServiceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("scoped");
        Assert.NotNull(vectorStore);
    }
}
