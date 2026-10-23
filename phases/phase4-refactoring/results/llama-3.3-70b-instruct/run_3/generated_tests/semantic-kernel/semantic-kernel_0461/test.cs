using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.VectorData.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MongoDB.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<Microsoft.SemanticKernel.Connectors.MongoDB.IEmbeddingGenerator, MockEmbeddingGenerator>()
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, Microsoft.Extensions.VectorData.MongoDB.MongoVectorStoreOptions?>(sp => null);

            // Act
            var options = Microsoft.Extensions.DependencyInjection.MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

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

            var optionsProvider = new Func<IServiceProvider, Microsoft.Extensions.VectorData.MongoDB.MongoVectorStoreOptions?>(sp => null);

            // Act
            var options = Microsoft.Extensions.DependencyInjection.MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        private class MockEmbeddingGenerator : Microsoft.SemanticKernel.Connectors.MongoDB.IEmbeddingGenerator
        {
            public Task<BsonDocument> GenerateAsync(object input)
            {
                throw new NotImplementedException();
            }
        }
    }
}
