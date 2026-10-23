using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        var options = new QdrantVectorStoreOptions { EmbeddingGenerator = mockEmbeddingGenerator };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Equal(mockEmbeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsAndNoService_ReturnsSameOptions()
    {
        // Arrange
        var options = new QdrantVectorStoreOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Null(result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsButServiceAvailable_CreatesNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var options = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(options, result);
        Assert.Equal(mockEmbeddingGenerator, result!.EmbeddingGenerator);
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
    public void GetCollectionOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        var options = new QdrantCollectionOptions { EmbeddingGenerator = mockEmbeddingGenerator };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Equal(mockEmbeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsButServiceAvailable_CreatesNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var options = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(options, result);
        Assert.Equal(mockEmbeddingGenerator, result!.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public IReadOnlyList<Embedding> GenerateEmbeddings(IReadOnlyList<string> texts) => Array.Empty<Embedding>();
        public IReadOnlyList<Embedding> GenerateEmbeddings(IReadOnlyList<object> textEmbeddings) => Array.Empty<Embedding>();
        public IAsyncEnumerable<Embedding> GenerateEmbeddingsAsync(IAsyncEnumerable<string> texts, CancellationToken cancellationToken = default) => System.Linq.AsyncEnumerable.Empty<Embedding>();
        public IAsyncEnumerable<Embedding> GenerateEmbeddingsAsync(IAsyncEnumerable<object> textEmbeddings, CancellationToken cancellationToken = default) => System.Linq.AsyncEnumerable.Empty<Embedding>();
        public object? GetService(Type serviceType, object? serviceKey) => null;
        public void Dispose() { }
    }
}
