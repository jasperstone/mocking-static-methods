using System;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_WithEmbeddingGenerator_ReturnsOptionsWithGenerator()
        {
            // Arrange
            var options = new PostgresVectorStoreOptions();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var embeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGenerator.Object);

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, null);

            // Assert
            Assert.Equal(options, result);
            Assert.Same(embeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WithoutEmbeddingGenerator_ReturnsOptionsWithoutGenerator()
        {
            // Arrange
            var options = new PostgresVectorStoreOptions();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns<IEmbeddingGenerator>(null);

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, null);

            // Assert
            Assert.Equal(options, result);
            Assert.Null(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_WithEmbeddingGenerator_ReturnsOptionsWithGenerator()
        {
            // Arrange
            var options = new PostgresCollectionOptions();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var embeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGenerator.Object);

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(mockServiceProvider.Object, null);

            // Assert
            Assert.Equal(options, result);
            Assert.Same(embeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_WithoutEmbeddingGenerator_ReturnsOptionsWithoutGenerator()
        {
            // Arrange
            var options = new PostgresCollectionOptions();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns<IEmbeddingGenerator>(null);

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(mockServiceProvider.Object, null);

            // Assert
            Assert.Equal(options, result);
            Assert.Null(result.EmbeddingGenerator);
        }
    }
}
