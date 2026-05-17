using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorProvided_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new QdrantCollectionOptions { EmbeddingGenerator = Mock.Of<IEmbeddingGenerator>() };
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == options.EmbeddingGenerator);

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvided_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new QdrantCollectionOptions();
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == null);

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorNotInOptionsButProvidedByServiceProvider_ReturnsNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var options = new QdrantCollectionOptions();
        var embeddingGenerator = Mock.Of<IEmbeddingGenerator>();
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == embeddingGenerator);

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, null);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(options, result);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }
}
