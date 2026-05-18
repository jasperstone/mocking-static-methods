using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using Xunit;

namespace VectorDataTests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator, MockEmbeddingGenerator>()
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new MongoVectorStoreOptions();

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

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

            var optionsProvider = (IServiceProvider sp) => new MongoVectorStoreOptions();

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        private class MockEmbeddingGenerator : IEmbeddingGenerator
        {
        }
    }
}
