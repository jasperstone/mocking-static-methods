using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Qdrant.Client;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_NoEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

        var optionsProviderMock = new Mock<Func<IServiceProvider, QdrantVectorStoreOptions?>>();
        optionsProviderMock.Setup(op => op.Invoke(It.IsAny<IServiceProvider>())).Returns((QdrantVectorStoreOptions?)null);

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProviderMock.Object);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_EmbeddingGeneratorAvailable_ReturnsOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
        serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGeneratorMock.Object);

        var optionsProviderMock = new Mock<Func<IServiceProvider, QdrantVectorStoreOptions?>>();
        var options = new QdrantVectorStoreOptions();
        optionsProviderMock.Setup(op => op.Invoke(It.IsAny<IServiceProvider>())).Returns(options);

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProviderMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
    }
}
