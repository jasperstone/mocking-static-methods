using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithEmbeddingGeneratorInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => new MongoVectorStoreOptions();

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Same(mockEmbeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenOptionsProviderReturnsNull_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => null;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Same(mockEmbeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenOptionsHasEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
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
    public void GetCollectionOptions_WithEmbeddingGeneratorInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => new MongoCollectionOptions();

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Same(mockEmbeddingGenerator, result.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public IReadOnlyList<float> GenerateEmbedding(ReadOnlyMemory<string> text)
            => Array.Empty<float>();

        public IReadOnlyList<float> GenerateEmbedding(ReadOnlyMemory<object> textEmbeddings)
            => Array.Empty<float>();

        public IAsyncEnumerable<float> GenerateEmbeddingsAsync(IAsyncEnumerable<string> texts, CancellationToken cancellationToken = default)
            => System.Linq.AsyncEnumerable.Empty<float>();

        public IAsyncEnumerable<float> GenerateEmbeddingsAsync(IAsyncEnumerable<object> textEmbeddings, CancellationToken cancellationToken = default)
            => System.Linq.AsyncEnumerable.Empty<float>();

        public void Dispose() { }
    }
}
