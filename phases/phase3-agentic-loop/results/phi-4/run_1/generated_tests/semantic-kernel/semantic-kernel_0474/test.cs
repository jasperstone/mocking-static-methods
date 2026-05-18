using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Qdrant.Client;
using Microsoft.Extensions.VectorData;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WhenOptionsProviderIsNull_ReturnsNull()
    {
        // Arrange
        var serviceProvider = Mock.Of<IServiceProvider>();

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_WhenOptionsProviderProvidesOptionsWithEmbeddingGenerator_ReturnsOptionsUnchanged()
    {
        // Arrange
        var serviceProvider = Mock.Of<IServiceProvider>();
        var options = new QdrantVectorStoreOptions { EmbeddingGenerator = Mock.Of<IEmbeddingGenerator>() };

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, sp => options);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_WhenOptionsProviderProvidesOptionsWithNullEmbeddingGeneratorAndServiceProviderHasEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == Mock.Of<IEmbeddingGenerator>());
        var options = new QdrantVectorStoreOptions();

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, sp => options);

        // Assert
        Assert.NotNull(result);
        Assert.Same(options.EmbeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenOptionsProviderProvidesOptionsWithNullEmbeddingGeneratorAndServiceProviderHasNoEmbeddingGenerator_ReturnsOptionsUnchanged()
    {
        // Arrange
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == null);
        var options = new QdrantVectorStoreOptions();

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, sp => options);

        // Assert
        Assert.Same(options, result);
    }
}
