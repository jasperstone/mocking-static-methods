using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class PostgresServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptions_AndNoService_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new PostgresVectorStoreOptions { Schema = "test" };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
        Assert.Null(result!.EmbeddingGenerator);
        Assert.Equal("test", result.Schema);
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
    public void GetStoreOptions_WithNoEmbeddingGeneratorInOptions_AndServiceAvailable_CreatesNewOptionsWithService()
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
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
        Assert.Equal("test", result.Schema);
    }

    [Fact]
    public void GetStoreOptions_WithNullOptionsProvider_ReturnsOptionsWithServiceIfAvailable()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.NotNull(result);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptions_AndNoService_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new PostgresCollectionOptions { Schema = "test" };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
        Assert.Null(result!.EmbeddingGenerator);
        Assert.Equal("test", result.Schema);
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
    public void GetCollectionOptions_WithNoEmbeddingGeneratorInOptions_AndServiceAvailable_CreatesNewOptionsWithService()
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
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
        Assert.Equal("test", result.Schema);
    }

    [Fact]
    public void GetCollectionOptions_WithNullOptionsProvider_ReturnsOptionsWithServiceIfAvailable()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.NotNull(result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }

    private sealed class MockEmbeddingGenerator : IEmbeddingGenerator
    {
        public IReadOnlyList<Embedding<float>> GenerateEmbeddings(IReadOnlyList<TextEmbeddingInput> inputs)
            => throw new NotImplementedException();

        public IAsyncEnumerable<Embedding<float>> GenerateEmbeddingsAsync(IAsyncEnumerable<TextEmbeddingInput> inputs, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
