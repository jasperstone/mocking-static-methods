using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithEmbeddingGeneratorInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var mockGenerator = new MockEmbeddingGenerator();
        var serviceProvider = CreateServiceProviderWithEmbeddingGenerator(mockGenerator);
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => new MongoVectorStoreOptions();

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Same(mockGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var originalOptions = new MongoVectorStoreOptions { EmbeddingGenerator = new MockEmbeddingGenerator() };
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }

    [Fact]
    public void GetStoreOptions_WhenNoEmbeddingGeneratorAvailable_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var originalOptions = new MongoVectorStoreOptions();
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
        Assert.Null(result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenOptionsProviderReturnsNull_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var mockGenerator = new MockEmbeddingGenerator();
        var serviceProvider = CreateServiceProviderWithEmbeddingGenerator(mockGenerator);
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => null;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Same(mockGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithEmbeddingGeneratorInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var mockGenerator = new MockEmbeddingGenerator();
        var serviceProvider = CreateServiceProviderWithEmbeddingGenerator(mockGenerator);
        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => new MongoCollectionOptions();

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Same(mockGenerator, result.EmbeddingGenerator);
    }

    private static IServiceProvider CreateServiceProviderWithEmbeddingGenerator(object generator)
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(object), generator);
        return services.BuildServiceProvider();
    }

    private class MockEmbeddingGenerator
    {
    }
}
