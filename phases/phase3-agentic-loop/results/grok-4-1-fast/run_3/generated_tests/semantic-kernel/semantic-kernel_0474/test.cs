using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WithOptionsProviderReturningNull_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = GetStoreOptions(serviceProvider, _ => null);

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

        // Act
        var result = GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsAndNoService_ReturnsSameOptions()
    {
        // Arrange
        var options = new QdrantVectorStoreOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsButServiceAvailable_ReturnsNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var options = new QdrantVectorStoreOptions();

        // Act
        var result = GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(options, result);
        Assert.Equal(mockEmbeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = GetCollectionOptions(serviceProvider, null);

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

        // Act
        var result = GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsButServiceAvailable_ReturnsNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var mockEmbeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var options = new QdrantCollectionOptions();

        // Act
        var result = GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(options, result);
        Assert.Equal(mockEmbeddingGenerator, result.EmbeddingGenerator);
    }

    private static QdrantVectorStoreOptions? GetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
    {
        var options = optionsProvider?.Invoke(sp);
        if (options?.EmbeddingGenerator is not null)
        {
            return options;
        }

        var embeddingGenerator = sp.GetService<IEmbeddingGenerator>();
        return embeddingGenerator is null
            ? options
            : new(options) { EmbeddingGenerator = embeddingGenerator };
    }

    private static QdrantCollectionOptions? GetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider)
    {
        var options = optionsProvider?.Invoke(sp);
        if (options?.EmbeddingGenerator is not null)
        {
            return options;
        }

        var embeddingGenerator = sp.GetService<IEmbeddingGenerator>();
        return embeddingGenerator is null
            ? options
            : new(options) { EmbeddingGenerator = embeddingGenerator };
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public IReadOnlyList<Embedding> GenerateEmbeddings(IReadOnlyList<string> data) => Array.Empty<Embedding>();
        public IReadOnlyList<Embedding> GenerateEmbeddings(IReadOnlyList<TextEmbeddingGenerationRecord> data) => Array.Empty<Embedding>();
        public object? GetService(Type serviceType, object? serviceKey) => null;
        public void Dispose() { }
    }
}
