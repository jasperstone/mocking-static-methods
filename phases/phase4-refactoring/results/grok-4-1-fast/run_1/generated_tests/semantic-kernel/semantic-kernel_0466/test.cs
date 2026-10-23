using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class PostgresServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WithOptionsProviderReturningNull_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => null;

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInDI_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new PostgresVectorStoreOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_WithEmbeddingGeneratorInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => new PostgresVectorStoreOptions();

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EmbeddingGenerator);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithEmbeddingGeneratorInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, PostgresCollectionOptions?> optionsProvider = _ => new PostgresCollectionOptions();

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EmbeddingGenerator);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    private sealed class MockEmbedding : Embedding<float>
    {
        public MockEmbedding() : base(Array.Empty<float>(), string.Empty) { }
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public IReadOnlyList<Embedding> GenerateEmbeddings(IReadOnlyList<TextEmbeddingInput> inputs)
            => new List<Embedding>();

        public IAsyncEnumerable<Embedding> GenerateEmbeddingsAsync(IAsyncEnumerable<TextEmbeddingInput> inputs, CancellationToken cancellationToken = default)
            => Enumerable.Empty<Embedding>().ToAsyncEnumerable();

        public void Dispose() { }
    }
}
