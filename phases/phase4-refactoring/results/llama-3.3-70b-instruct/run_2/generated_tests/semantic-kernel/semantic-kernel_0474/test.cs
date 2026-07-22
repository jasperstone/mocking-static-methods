using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.SemanticKernel;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<Microsoft.SemanticKernel.IEmbeddingGenerator, MockEmbeddingGenerator>()
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new QdrantVectorStoreOptions();

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
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

        private class MockEmbeddingGenerator : Microsoft.SemanticKernel.IEmbeddingGenerator
        {
            public void GenerateEmbedding(object input)
            {
                throw new NotImplementedException();
            }
        }
    }
}
