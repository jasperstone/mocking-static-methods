using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using MongoDB.Driver;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WithOptionsHavingEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new MongoVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
        var serviceProvider = new ServiceCollection().AddSingleton<IEmbeddingGenerator>(embeddingGenerator).BuildServiceProvider();

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsButInServiceProvider_InjectsEmbeddingGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var originalOptions = new MongoVectorStoreOptions(); // No embedding generator
        var serviceProvider = new ServiceCollection().AddSingleton<IEmbeddingGenerator>(embeddingGenerator).BuildServiceProvider();

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => originalOptions);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorAnywhere_ReturnsOriginalOptions()
    {
        // Arrange
        var originalOptions = new MongoVectorStoreOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider(); // No IEmbeddingGenerator

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => originalOptions);

        // Assert
        Assert.Same(originalOptions, result);
        Assert.Null(result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithOptionsHavingEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new MongoCollectionOptions { EmbeddingGenerator = embeddingGenerator };
        var serviceProvider = new ServiceCollection().AddSingleton<IEmbeddingGenerator>(embeddingGenerator).BuildServiceProvider();

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsButInServiceProvider_InjectsEmbeddingGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var originalOptions = new MongoCollectionOptions(); // No embedding generator
        var serviceProvider = new ServiceCollection().AddSingleton<IEmbeddingGenerator>(embeddingGenerator).BuildServiceProvider();

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => originalOptions);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(originalOptions, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorAnywhere_ReturnsOriginalOptions()
    {
        // Arrange
        var originalOptions = new MongoCollectionOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider(); // No IEmbeddingGenerator

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => originalOptions);

        // Assert
        Assert.Same(originalOptions, result);
        Assert.Null(result!.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator
    {
        public IReadOnlyList<float> GenerateEmbedding(ReadOnlyMemory<string> text)
            => throw new NotImplementedException();

        public IAsyncEnumerable<EmbeddingGenerationMetadata> GenerateEmbeddingsAsync(
            IAsyncEnumerable<TextEmbeddingGenerationRequest> requests,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
