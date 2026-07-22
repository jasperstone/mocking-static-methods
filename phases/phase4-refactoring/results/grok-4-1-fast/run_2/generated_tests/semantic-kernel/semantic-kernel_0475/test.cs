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
    public void GetCollectionOptions_WithOptionsHavingEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        var options = new QdrantCollectionOptions();
        options.EmbeddingGenerator = mockEmbeddingGenerator;
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Same(mockEmbeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_NoEmbeddingGeneratorInOptionsOrDI_ReturnsOriginalOptions()
    {
        // Arrange
        var originalOptions = new QdrantCollectionOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
        Assert.Null(result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_NoEmbeddingGeneratorInOptionsButPresentInDI_ReturnsNewOptionsWithDIEmbeddingGenerator()
    {
        // Arrange
        var diEmbeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(diEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.Same(diEmbeddingGenerator, result!.EmbeddingGenerator);
    }

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
    public void GetStoreOptions_WithOptionsHavingEmbeddingGenerator_ReturnsOriginalOptions()
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
        Assert.Same(mockEmbeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_NoEmbeddingGeneratorInOptionsButPresentInDI_ReturnsNewOptionsWithDIEmbeddingGenerator()
    {
        // Arrange
        var diEmbeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(diEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.Same(diEmbeddingGenerator, result!.EmbeddingGenerator);
    }
}

public class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
{
    public IReadOnlyList<Embedding<float>> GenerateEmbeddings(IReadOnlyList<string> texts) => 
        texts.Select(t => new Embedding<float>(new float[128])).ToList();

    public IReadOnlyList<Embedding<float>> GenerateEmbeddings(IReadOnlyList<TextEmbeddingGenerationRecord> textEmbeddings) => 
        textEmbeddings.Select(t => new Embedding<float>(t.Embedding ?? new float[128])).ToList();

    public IAsyncEnumerable<Embedding<float>> GenerateEmbeddingsAsync(IAsyncEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        return GenerateEmbeddingsAsyncCore(texts, cancellationToken);
    }

    private async IAsyncEnumerable<Embedding<float>> GenerateEmbeddingsAsyncCore(IAsyncEnumerable<string> texts, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var text in texts.WithCancellation(cancellationToken))
        {
            yield return new Embedding<float>(new float[128]);
        }
    }

    public IAsyncEnumerable<Embedding<float>> GenerateEmbeddingsAsync(IAsyncEnumerable<TextEmbeddingGenerationRecord> textEmbeddings, CancellationToken cancellationToken = default)
    {
        return GenerateEmbeddingsAsyncCore(textEmbeddings, cancellationToken);
    }

    private async IAsyncEnumerable<Embedding<float>> GenerateEmbeddingsAsyncCore(IAsyncEnumerable<TextEmbeddingGenerationRecord> textEmbeddings, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var te in textEmbeddings.WithCancellation(cancellationToken))
        {
            yield return new Embedding<float>(te.Embedding ?? new float[128]);
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}
