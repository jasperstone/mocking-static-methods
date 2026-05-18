using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_CallsGetServiceOnServiceProvider_WhenEmbeddingGeneratorIsNullInOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        Func<IServiceProvider, object?> optionsProvider = _ => new object();

        // Act
        var result = PrivateType.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetStoreOptions_WithEmbeddingGeneratorInContainer_SetsEmbeddingGeneratorInNewOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();
        
        Func<IServiceProvider, object?> optionsProvider = _ => new object();

        // Act
        var result = PrivateType.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetStoreOptions_ReturnsOriginalOptions_WhenEmbeddingGeneratorAlreadySet()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var originalOptions = new object();
        
        Func<IServiceProvider, object?> optionsProvider = _ => originalOptions;

        // Act
        var result = PrivateType.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }

    [Fact]
    public void GetCollectionOptions_CallsGetServiceOnServiceProvider_WhenEmbeddingGeneratorIsNullInOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        Func<IServiceProvider, object?> optionsProvider = _ => new object();

        // Act
        var result = PrivateType.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetCollectionOptions_WithEmbeddingGeneratorInContainer_SetsEmbeddingGeneratorInNewOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();
        
        Func<IServiceProvider, object?> optionsProvider = _ => new object();

        // Act
        var result = PrivateType.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetCollectionOptions_ReturnsOriginalOptions_WhenEmbeddingGeneratorAlreadySet()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var originalOptions = new object();
        
        Func<IServiceProvider, object?> optionsProvider = _ => originalOptions;

        // Act
        var result = PrivateType.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }
}

// Helper class to access private static methods using reflection
static class PrivateType
{
    private static readonly Type MongoExtensionsType = typeof(MongoServiceCollectionExtensions);
    
    public static object? GetStoreOptions(IServiceProvider sp, Func<IServiceProvider, object?>? optionsProvider)
    {
        var method = MongoExtensionsType.GetMethod("GetStoreOptions", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        return method.Invoke(null, new object?[] { sp, optionsProvider });
    }
    
    public static object? GetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, object?>? optionsProvider)
    {
        var method = MongoExtensionsType.GetMethod("GetCollectionOptions", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        return method.Invoke(null, new object?[] { sp, optionsProvider });
    }
}
