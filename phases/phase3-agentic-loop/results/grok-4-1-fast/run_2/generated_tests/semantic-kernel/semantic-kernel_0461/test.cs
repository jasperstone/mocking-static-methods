using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;

namespace Microsoft.Extensions.DependencyInjection;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_CallsGetServiceOnServiceProvider_WhenEmbeddingGeneratorIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => null;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_ReturnsOptionsWithEmbeddingGeneratorFromDI_WhenOptionsEmbeddingGeneratorIsNull()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        services.TryAddSingleton(mockEmbeddingGenerator.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var options = new MongoVectorStoreOptions();
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EmbeddingGenerator);
        Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        Assert.NotSame(options, result); // New instance created
    }

    [Fact]
    public void GetStoreOptions_ReturnsOriginalOptions_WhenOptionsEmbeddingGeneratorIsNotNull()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        var originalOptions = new MongoVectorStoreOptions { EmbeddingGenerator = mockEmbeddingGenerator.Object };
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }

    [Fact]
    public void GetCollectionOptions_CallsGetServiceOnServiceProvider_WhenEmbeddingGeneratorIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => null;

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_ReturnsOptionsWithEmbeddingGeneratorFromDI_WhenOptionsEmbeddingGeneratorIsNull()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        services.TryAddSingleton(mockEmbeddingGenerator.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var options = new MongoCollectionOptions();
        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EmbeddingGenerator);
        Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        Assert.NotSame(options, result); // New instance created
    }

    [Fact]
    public void GetCollectionOptions_ReturnsOriginalOptions_WhenOptionsEmbeddingGeneratorIsNotNull()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        var originalOptions = new MongoCollectionOptions { EmbeddingGenerator = mockEmbeddingGenerator.Object };
        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }
}
