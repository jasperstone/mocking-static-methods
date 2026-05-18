using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_WhenEmbeddingGeneratorProvided_ShouldReturnOptions()
        {
            // Arrange
            var options = new QdrantCollectionOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(options.EmbeddingGenerator);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvidedAndAvailable_ShouldReturnOptionsWithGenerator()
        {
            // Arrange
            var options = new QdrantCollectionOptions();
            var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGenerator);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvidedAndNotAvailable_ShouldReturnOriginalOptions()
        {
            // Arrange
            var options = new QdrantCollectionOptions();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetCollectionOptions_WhenOptionsProviderReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, sp => null);

            // Assert
            Assert.Null(result);
        }
    }
}
