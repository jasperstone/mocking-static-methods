using System;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyData;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class CosmosNoSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_RegistersCorrectDescriptors_WithDatabaseFromDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new Database(new CosmosClient("fake"), "fake"));

        // Act
        var result = services.AddKeyedCosmosNoSqlVectorStore(null);

        // Assert
        Assert.Same(services, result);
        
        var cosmosDescriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(CosmosNoSqlVectorStore));
        Assert.NotNull(cosmosDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, cosmosDescriptor.Lifetime);
        
        var vectorDescriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(VectorStore));
        Assert.NotNull(vectorDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, vectorDescriptor.Lifetime);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_FactoryUsesGetRequiredService_WhenResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDatabase = new MockDatabase();
        services.AddSingleton(mockDatabase);

        // Act
        services.AddKeyedCosmosNoSqlVectorStore(null);
        using var provider = services.BuildServiceProvider();
        
        // This triggers the factory lambda containing sp.GetRequiredService<Database>()
        _ = provider.GetRequiredKeyedService<CosmosNoSqlVectorStore>(null);

        // Assert - if we reach here, GetRequiredService succeeded
        Assert.NotNull(mockDatabase);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_CallsKeyedVersion_WithNullKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new Database(new CosmosClient("fake"), "fake"));

        // Act
        var result = services.AddCosmosNoSqlVectorStore();

        // Assert - same registrations as keyed version with null key
        var cosmosDescriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(CosmosNoSqlVectorStore));
        Assert.NotNull(cosmosDescriptor);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_ThrowsArgumentNullException_WhenServicesNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => ((IServiceCollection)null!).AddKeyedCosmosNoSqlVectorStore(null));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddKeyedCosmosNoSqlVectorStore(null, "connstr", "dbname");

        // Assert
        Assert.Same(services, result);
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(CosmosNoSqlVectorStore));
        Assert.NotNull(descriptor);
    }
}

public class MockDatabase : Database
{
    public MockDatabase() : base(new CosmosClient("fake"), "fake") { }
}
