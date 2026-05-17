using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_WithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<Microsoft.SemanticKernel.Connectors.Qdrant.IEmbeddingGenerator>(Mock.Of<Microsoft.SemanticKernel.Connectors.Qdrant.IEmbeddingGenerator>())
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(
                sp => new QdrantVectorStoreOptions());

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WithoutEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(
                sp => new QdrantVectorStoreOptions());

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }
    }
}
