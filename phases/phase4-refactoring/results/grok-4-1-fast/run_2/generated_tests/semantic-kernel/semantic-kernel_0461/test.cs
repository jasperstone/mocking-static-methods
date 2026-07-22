using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithEmbeddingGeneratorInDI_ReturnsOptionsWithGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        var embeddingGenerator = new MockEmbeddingGenerator();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => new MongoVectorStoreOptions();

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithUserProvidedEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var userEmbeddingGenerator = new MockEmbeddingGenerator();
        var userOptions = new MongoVectorStoreOptions { EmbeddingGenerator = userEmbeddingGenerator };
        Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => userOptions;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.Same(userOptions, result);
        Assert.Equal(userEmbeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_NoEmbeddingGeneratorAvailable_ReturnsOriginalOptions()
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
        Assert.Null(result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_NullOptionsProvider_ReturnsOptionsWithDIEmbeddingGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        var embeddingGenerator = new MockEmbeddingGenerator();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, MongoVectorStoreOptions?>? optionsProvider = null;

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithEmbeddingGeneratorInDI_ReturnsOptionsWithGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        var embeddingGenerator = new MockEmbeddingGenerator();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, MongoCollectionOptions?> optionsProvider = _ => new MongoCollectionOptions();

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(embeddingGenerator, result.EmbeddingGenerator);
    }

    private class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public object? GetService(Type serviceType, object? serviceKey) => null;

        public IReadOnlyList<float> GenerateEmbedding(ReadOnlyMemory<string> text)
            => new List<float>();

        public IReadOnlyList<float> GenerateEmbedding(ReadOnlyMemory<object> textEmbeddings)
            => new List<float>();

        public IAsyncEnumerable<float[]> GenerateEmbeddingsAsync(IAsyncEnumerable<string> texts, CancellationToken cancellationToken = default)
            => System.Linq.AsyncEnumerable.Empty<float[]>();

        public IAsyncEnumerable<float[]> GenerateEmbeddingsAsync(IAsyncEnumerable<object> textEmbeddings, CancellationToken cancellationToken = default)
            => System.Linq.AsyncEnumerable.Empty<float[]>();

        public void Dispose() { }
    }
}
