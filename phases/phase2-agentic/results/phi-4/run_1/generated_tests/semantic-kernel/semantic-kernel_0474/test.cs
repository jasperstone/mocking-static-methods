using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Qdrant.Client;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_WithProvidedEmbeddingGenerator_ReturnsOptions()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = Mock.Of<IEmbeddingGenerator>() };
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => options);
            var serviceProvider = Mock.Of<IServiceProvider>();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_WithoutProvidedEmbeddingGenerator_WithServiceProvider_ReturnsOptionsWithGenerator()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions();
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => options);
            var embeddingGenerator = Mock.Of<IEmbeddingGenerator>();
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGenerator);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WithoutProvidedEmbeddingGenerator_WithoutServiceProvider_ReturnsOriginalOptions()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions();
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => options);
            var serviceProvider = Mock.Of<IServiceProvider>();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_WithNullOptionsProvider_ReturnsNull()
        {
            // Arrange
            var serviceProvider = Mock.Of<IServiceProvider>();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

            // Assert
            Assert.Null(result);
        }
    }
}
