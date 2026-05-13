using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_WhenOptionsProviderReturnsNullAndNoEmbeddingGenerator_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns<IEmbeddingGenerator>(null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider.Object, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCollectionOptions_WhenOptionsProviderReturnsOptionsAndHasEmbeddingGenerator_ReturnsOptionsUnchanged()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var options = new QdrantCollectionOptions { HasNamedVectors = true };
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns<IEmbeddingGenerator>(null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider.Object, sp => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetCollectionOptions_WhenOptionsProviderReturnsNullAndEmbeddingGeneratorIsProvided_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var embeddingGenerator = new Mock<IEmbeddingGenerator>();
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGenerator.Object);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_WhenOptionsProviderReturnsOptionsAndEmbeddingGeneratorIsProvided_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var options = new QdrantCollectionOptions { HasNamedVectors = true };
            var embeddingGenerator = new Mock<IEmbeddingGenerator>();
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGenerator.Object);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator.Object, result.EmbeddingGenerator);
            Assert.Equal(options.HasNamedVectors, result.HasNamedVectors);
        }
    }
}
