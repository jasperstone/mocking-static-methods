using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;
using Microsoft.Extensions.VectorData;

namespace Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator, MockEmbeddingGenerator>()
                .BuildServiceProvider();
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);
            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);
            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);
            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);
            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_OptionsProviderReturnsOptions_ReturnsOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => new QdrantCollectionOptions());
            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);
            // Assert
            Assert.NotNull(options);
        }

        [Fact]
        public void GetCollectionOptions_OptionsProviderReturnsOptionsWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => new QdrantCollectionOptions { EmbeddingGenerator = new MockEmbeddingGenerator() });
            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);
            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }

        private class MockEmbeddingGenerator : IEmbeddingGenerator
        {
        }
    }
}
