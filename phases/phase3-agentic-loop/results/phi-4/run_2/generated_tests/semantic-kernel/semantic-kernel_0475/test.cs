using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorProvided_ReturnsOptions()
    {
        // Arrange
        var options = new QdrantCollectionOptions { EmbeddingGenerator = Mock.Of<IEmbeddingGenerator>() };
        var optionsProvider = new Mock<Func<IServiceProvider, QdrantCollectionOptions?>>();
        optionsProvider.Setup(p => p(It.IsAny<IServiceProvider>())).Returns(options);

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(Mock.Of<IServiceProvider>(), optionsProvider.Object);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvidedAndServiceReturnsNull_ReturnsOptions()
    {
        // Arrange
        var options = new QdrantCollectionOptions();
        var optionsProvider = new Mock<Func<IServiceProvider, QdrantCollectionOptions?>>();
        optionsProvider.Setup(p => p(It.IsAny<IServiceProvider>())).Returns(options);

        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService(typeof(IEmbeddingGenerator)) == null);

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider.Object);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvidedAndServiceReturnsGenerator_ReturnsOptionsWithGenerator()
    {
        // Arrange
        var options = new QdrantCollectionOptions();
        var optionsProvider = new Mock<Func<IServiceProvider, QdrantCollectionOptions?>>();
        optionsProvider.Setup(p => p(It.IsAny<IServiceProvider>())).Returns(options);

        var generator = Mock.Of<IEmbeddingGenerator>();
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService(typeof(IEmbeddingGenerator)) == generator);

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(generator, result.EmbeddingGenerator);
    }
}
