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
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsOrServiceProvider_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new PostgresVectorStoreOptions { Schema = "test" };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
        Assert.Equal("test", result!.Schema);
        Assert.Null(result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithEmbeddingGeneratorInOptions_ReturnsOriginalOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptionsButInServiceProvider_ReturnsNewOptionsWithServiceProviderGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var options = new PostgresVectorStoreOptions { Schema = "test" };

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.NotSame(options, result);
        Assert.Equal("test", result!.Schema);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsOrServiceProvider_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new PostgresCollectionOptions { Schema = "test" };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
        Assert.Equal("test", result!.Schema);
        Assert.Null(result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithEmbeddingGeneratorInOptions_ReturnsOriginalOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new PostgresCollectionOptions { EmbeddingGenerator = embeddingGenerator };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptionsButInServiceProvider_ReturnsNewOptionsWithServiceProviderGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        var options = new PostgresCollectionOptions { Schema = "test" };

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.NotSame(options, result);
        Assert.Equal("test", result!.Schema);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator
    {
        public IReadOnlyList<float> GenerateEmbedding(ReadOnlyMemory<string> text)
            => new List<float>();

        public IAsyncEnumerable<object> GenerateEmbeddingsAsync(
            IAsyncEnumerable<object> requests,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public void Dispose() { }
    }
}
