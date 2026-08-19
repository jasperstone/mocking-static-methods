using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;

namespace VectorData.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderHasEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGenerator = new Mock<IEmbeddingGenerator>();
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGenerator.Object);

            var optionsProvider = new Func<IServiceProvider, MongoVectorStoreOptions?>((sp) => null);

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Same(embeddingGenerator.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ServiceProviderDoesNotHaveEmbeddingGenerator_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            var optionsProvider = new Func<IServiceProvider, MongoVectorStoreOptions?>((sp) => null);

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, optionsProvider);

            // Assert
            Assert.Null(options);
        }
    }
}
