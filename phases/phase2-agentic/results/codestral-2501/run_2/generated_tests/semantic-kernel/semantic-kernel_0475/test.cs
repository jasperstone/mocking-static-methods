using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithoutEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnProvidedOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, sp => options);

            // Assert
            Assert.Same(options, result);
        }
    }
}
