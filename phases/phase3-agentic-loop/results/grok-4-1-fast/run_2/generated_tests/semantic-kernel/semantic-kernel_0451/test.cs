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
    public void AddCosmosNoSqlVectorStore_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null!).AddCosmosNoSqlVectorStore());
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_ValidParameters_AddsServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCosmosNoSqlVectorStore();

        // Assert
        Assert.Same(services, result);
        var cosmosDescriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(CosmosNoSqlVectorStore)));
        Assert.Null(cosmosDescriptor.ServiceKey);
        Assert.Equal(ServiceLifetime.Singleton, cosmosDescriptor.Lifetime);

        var vectorDescriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(VectorStore)));
        Assert.Null(vectorDescriptor.ServiceKey);
        Assert.Equal(ServiceLifetime.Singleton, vectorDescriptor.Lifetime);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null!).AddKeyedCosmosNoSqlVectorStore(null));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_ValidParametersWithKey_AddsKeyedServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var key = new object();

        // Act
        var result = services.AddKeyedCosmosNoSqlVectorStore(key);

        // Assert
        Assert.Same(services, result);
        var cosmosDescriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(CosmosNoSqlVectorStore)));
        Assert.Equal(key, cosmosDescriptor.ServiceKey);
        Assert.Equal(ServiceLifetime.Singleton, cosmosDescriptor.Lifetime);

        var vectorDescriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(VectorStore)));
        Assert.Equal(key, vectorDescriptor.ServiceKey);
        Assert.Equal(ServiceLifetime.Singleton, vectorDescriptor.Lifetime);
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_WithConnectionString_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null!).AddCosmosNoSqlVectorStore("conn", "db"));
    }

    [Fact]
    public void AddCosmosNoSqlVectorStore_WithConnectionString_ValidParameters_AddsServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCosmosNoSqlVectorStore("connectionString", "databaseName");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(CosmosNoSqlVectorStore)));
        Assert.Null(descriptor.ServiceKey);
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null!).AddKeyedCosmosNoSqlVectorStore(new object(), "conn", "db"));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_EmptyConnectionString_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Assert.ThrowsAny<ArgumentException>(() => services.AddKeyedCosmosNoSqlVectorStore(new object(), "", "db"));
    }

    [Fact]
    public void AddKeyedCosmosNoSqlVectorStore_WithConnectionString_EmptyDatabaseName_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Assert.ThrowsAny<ArgumentException>(() => services.AddKeyedCosmosNoSqlVectorStore(new object(), "conn", ""));
    }
}
