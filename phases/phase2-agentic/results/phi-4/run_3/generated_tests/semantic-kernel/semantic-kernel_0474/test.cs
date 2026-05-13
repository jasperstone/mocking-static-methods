using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WhenEmbeddingGeneratorProvided_ShouldReturnOptionsWithGenerator()
    {
        // Arrange
        var options = new QdrantVectorStoreOptions();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();

        mockServiceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(mockEmbeddingGenerator.Object);

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, null);

        // Assert
        Assert.NotNull(result);
        Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenEmbeddingGeneratorNotProvided_ShouldReturnOriginalOptions()
    {
        // Arrange
        var options = new QdrantVectorStoreOptions();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockServiceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, null);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.EmbeddingGenerator);
    }
}
