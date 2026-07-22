using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using Xunit;

namespace VectorData.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            MongoVectorStoreOptions? options = null;
            Func<IServiceProvider, MongoVectorStoreOptions?>? optionsProvider = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => MongoServiceCollectionExtensions.GetStoreOptions(null, optionsProvider));
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderIsNull_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

            // Assert
            Assert.Null(options);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderIsNotNull_ReturnsOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var options = new MongoVectorStoreOptions();
            Func<IServiceProvider, MongoVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_EmbeddingGeneratorIsNotNull_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            serviceProvider.GetService<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }
    }
}
