using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Abstractions;
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
    public void GetStoreOptions_NoEmbeddingGeneratorInContainer_ReturnsOriginalOptions()
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
    public void GetStoreOptions_WithEmbeddingGeneratorInContainer_InjectsIntoNewOptionsCopy()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.TryAddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new PostgresVectorStoreOptions();
        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Equal(originalOptions.Schema, result!.Schema);
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
    public void GetCollectionOptions_WithEmbeddingGeneratorInContainer_InjectsIntoNewOptionsCopy()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.TryAddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new PostgresCollectionOptions();
        Func<IServiceProvider, PostgresCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public IReadOnlyList<float> GenerateEmbeddings(IReadOnlyList<string> texts) => Array.Empty<float>();
        public IAsyncEnumerable<Embedding<float>> GenerateEmbeddingsAsync(IAsyncEnumerable<string> texts, CancellationToken cancellationToken = default)
            => texts.Select(t => new Embedding<float>(Array.Empty<float>())).ToAsyncEnumerable();
        public void Dispose() { }
    }
}
