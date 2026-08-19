using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var options = new QdrantVectorStoreOptions();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(mockServiceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var options = new QdrantCollectionOptions();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(mockServiceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }
    }
}
