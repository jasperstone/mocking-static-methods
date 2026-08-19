using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithNoOptionsProvider_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WithNullOptionsFromProvider_ReturnsNull()
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
    public void GetStoreOptions_WithOptionsHavingEmbeddingGenerator_ReturnsOriginal()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var originalOptions = new MongoVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsButInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();
        var originalOptions = new MongoVectorStoreOptions();
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorAnywhere_ReturnsOriginalOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var originalOptions = new MongoVectorStoreOptions();
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }

    [Fact]
    public void GetCollectionOptions_WithNoOptionsProvider_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithNullOptionsFromProvider_ReturnsNull()
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
    public void GetCollectionOptions_WithOptionsHavingEmbeddingGenerator_ReturnsOriginal()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var originalOptions = new MongoCollectionOptions { EmbeddingGenerator = embeddingGenerator };
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsButInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();
        var originalOptions = new MongoCollectionOptions();
        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorAnywhere_ReturnsOriginalOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var originalOptions = new MongoCollectionOptions();
        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }

    private class MockEmbeddingGenerator : IEmbeddingGenerator
    {
        public IReadOnlyList<float> GenerateEmbedding(ReadOnlyMemory<string> text)
            => new List<float>();

        public IAsyncEnumerable<EmbeddingGeneration> GenerateEmbeddingAsync(
            IAsyncEnumerable<TextEmbeddingGenerationRequest> data, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
    }
}
