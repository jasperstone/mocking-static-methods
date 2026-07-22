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
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => new QdrantCollectionOptions());

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(mockServiceProvider.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenEmbeddingGeneratorIsNotAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => new QdrantCollectionOptions());

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(mockServiceProvider.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }
    }
}
