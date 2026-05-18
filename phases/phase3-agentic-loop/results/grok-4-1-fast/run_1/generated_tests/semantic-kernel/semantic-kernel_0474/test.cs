using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WithOptionsProviderReturningNull_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => null;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsAndNoneInDI_ReturnsOriginalOptions()
    {
        // Arrange
        var originalOptions = new QdrantVectorStoreOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsButPresentInDI_ReturnsNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithOptionsProviderReturningNull_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => null;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new QdrantCollectionOptions();
        options.EmbeddingGenerator = embeddingGenerator;
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsButPresentInDI_ReturnsNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public IReadOnlyList<Embedding> GenerateEmbeddings(IReadOnlyList<TextEmbeddingGenerationRequest> requests)
            => Array.Empty<Embedding>();

        public IAsyncEnumerable<Embedding> GenerateEmbeddingsAsync(IAsyncEnumerable<TextEmbeddingGenerationRequest> requests, CancellationToken cancellationToken = default)
            => AsyncEnumerable.Empty<Embedding>();

        public void Dispose() { }

        public object? GetService(Type serviceType, object? serviceKey) => null;
    }
}
