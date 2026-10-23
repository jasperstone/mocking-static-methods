using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_NullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_OptionsProviderReturnsNull_ReturnsNull()
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
    public void GetStoreOptions_OptionsHasEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
        var serviceProvider = new ServiceCollection()
            .AddSingleton(embeddingGenerator)
            .BuildServiceProvider();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_NoEmbeddingInOptionsNoDI_ReturnsOriginalOptions()
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
    public void GetStoreOptions_NoEmbeddingInOptionsButInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var originalOptions = new QdrantVectorStoreOptions();
        var serviceProvider = new ServiceCollection()
            .AddSingleton(embeddingGenerator)
            .BuildServiceProvider();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_NullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions<string, object>(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_OptionsHasEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new QdrantCollectionOptions { EmbeddingGenerator = embeddingGenerator };
        var serviceProvider = new ServiceCollection()
            .AddSingleton(embeddingGenerator)
            .BuildServiceProvider();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions<string, object>(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_NoEmbeddingInOptionsButInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var originalOptions = new QdrantCollectionOptions();
        var serviceProvider = new ServiceCollection()
            .AddSingleton(embeddingGenerator)
            .BuildServiceProvider();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions<string, object>(serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator
    {
        public IReadOnlyList<Embedding<float>> GenerateEmbeddings(IReadOnlyList<TextEmbeddingInput<string>> inputs)
            => Array.Empty<Embedding<float>>();

        public IAsyncEnumerable<Embedding<float>> GenerateEmbeddingsAsync(
            IAsyncEnumerable<TextEmbeddingInput<string>> inputs, 
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
