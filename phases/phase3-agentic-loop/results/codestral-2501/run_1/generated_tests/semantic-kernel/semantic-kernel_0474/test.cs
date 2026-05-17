using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetService_ShouldReturnEmbeddingGenerator_WhenEmbeddingGeneratorIsNotProvided()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetService_ShouldReturnOptions_WhenEmbeddingGeneratorIsProvided()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetService_ShouldReturnNull_WhenNoEmbeddingGeneratorIsAvailable()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Null(result);
        }
    }
}
