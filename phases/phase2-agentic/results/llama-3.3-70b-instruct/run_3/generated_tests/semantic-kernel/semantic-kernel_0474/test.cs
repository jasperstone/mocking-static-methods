using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator>(Mock.Of<IEmbeddingGenerator>())
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new QdrantVectorStoreOptions();

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new QdrantVectorStoreOptions();

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderReturnsNull_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = null;

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.Null(options);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderReturnsOptionsWithEmbeddingGenerator_ReturnsOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var embeddingGenerator = Mock.Of<IEmbeddingGenerator>();
            var optionsProvider = (IServiceProvider sp) => new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Same(embeddingGenerator, options.EmbeddingGenerator);
        }
    }
}
