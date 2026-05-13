using System;
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
        public void GetStoreOptions_WhenEmbeddingGeneratorNotRegistered_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);
            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, sp => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_WhenEmbeddingGeneratorRegistered_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);
            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WhenOptionsProviderReturnsNull_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, sp => null);

            // Assert
            Assert.Null(result);
        }
    }
}
