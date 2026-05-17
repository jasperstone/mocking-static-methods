using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace VectorData.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_OptionsProviderIsNull_EmbeddingGeneratorIsReturned()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<Microsoft.SemanticKernel.IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.SemanticKernel.IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var options = Microsoft.Extensions.DependencyInjection.QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(options);
            Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_OptionsProviderIsNull_EmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.SemanticKernel.IEmbeddingGenerator))).Returns(null);

            // Act
            var options = Microsoft.Extensions.DependencyInjection.QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Null(options);
        }

        [Fact]
        public void GetCollectionOptions_OptionsProviderIsNotNull_EmbeddingGeneratorIsReturned()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<Microsoft.SemanticKernel.IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.SemanticKernel.IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);
            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = sp => new QdrantCollectionOptions();

            // Act
            var options = Microsoft.Extensions.DependencyInjection.QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_OptionsProviderIsNotNull_EmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.SemanticKernel.IEmbeddingGenerator))).Returns(null);
            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = sp => new QdrantCollectionOptions();

            // Act
            var options = Microsoft.Extensions.DependencyInjection.QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }
    }
}
