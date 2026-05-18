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
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOriginalOptionsIfEmbeddingGeneratorIsNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            var options = new QdrantCollectionOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(mockServiceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOriginalOptionsIfEmbeddingGeneratorIsNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            var options = new QdrantCollectionOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(mockServiceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(options, result);
        }
    }
}
