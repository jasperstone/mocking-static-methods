using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new MockServiceProvider();

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WithOptionsProviderReturningNull_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new MockServiceProvider();
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
        var serviceProvider = new MockServiceProvider();
        var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsAndNoneInServiceProvider_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProvider = new MockServiceProvider();
        var options = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Null(result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsButPresentInServiceProvider_ReturnsNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var serviceProvider = new MockServiceProvider(embeddingGenerator);
        var originalOptions = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.NotNull(result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new MockServiceProvider();

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var serviceProvider = new MockServiceProvider();
        var options = new QdrantCollectionOptions { EmbeddingGenerator = embeddingGenerator };
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsButPresentInServiceProvider_ReturnsNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var serviceProvider = new MockServiceProvider(embeddingGenerator);
        var originalOptions = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.NotNull(result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    private sealed class MockServiceProvider : IServiceProvider
    {
        private readonly IEmbeddingGenerator? _embeddingGenerator;

        public MockServiceProvider(IEmbeddingGenerator? embeddingGenerator = null)
        {
            _embeddingGenerator = embeddingGenerator;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IEmbeddingGenerator))
            {
                return _embeddingGenerator;
            }
            return null;
        }
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IServiceProvider, IDisposable
    {
        public IReadOnlyList<Embedding> GenerateEmbeddings(IReadOnlyList<TextEmbeddingRequest> requests)
            => Array.Empty<Embedding>();

        public IAsyncEnumerable<Embedding> GenerateEmbeddingsAsync(
            IAsyncEnumerable<TextEmbeddingRequest> requests,
            CancellationToken cancellationToken = default)
            => System.Linq.AsyncEnumerable.Empty<Embedding>();

        object? IServiceProvider.GetService(Type serviceType) => null;

        public void Dispose() { }
    }
}
