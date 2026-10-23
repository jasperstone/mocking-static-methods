using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
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
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsAndNoneInDI_ReturnsOriginalOptions()
    {
        // Arrange
        var originalOptions = new PostgresVectorStoreOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(originalOptions, result);
        Assert.Null(result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsButPresentInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new PostgresVectorStoreOptions();
        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
        Assert.Equal("public", result.Schema);
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
    public void GetCollectionOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new PostgresCollectionOptions();
        options.EmbeddingGenerator = embeddingGenerator;
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, PostgresCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsButPresentInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new PostgresCollectionOptions();
        Func<IServiceProvider, PostgresCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public IReadOnlyList<Embedding<float>> GenerateEmbeddings(IReadOnlyList<object> inputs)
            => Array.Empty<Embedding<float>>();

        public IAsyncEnumerable<Embedding<float>> GenerateEmbeddingsAsync(IAsyncEnumerable<object> inputs, CancellationToken cancellationToken = default)
            => System.Linq.AsyncEnumerable.Empty<Embedding<float>>();

        public void Dispose() { }
    }
}
