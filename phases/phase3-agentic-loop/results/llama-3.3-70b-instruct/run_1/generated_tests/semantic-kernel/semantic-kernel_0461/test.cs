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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<Microsoft.Extensions.AI.IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var optionsProvider = new Func<IServiceProvider, MongoVectorStoreOptions?>(sp => new MongoVectorStoreOptions());

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
            Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator))).Returns(null);

            var optionsProvider = new Func<IServiceProvider, MongoVectorStoreOptions?>(sp => new MongoVectorStoreOptions());

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderReturnsNull_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator))).Returns(null);

            var optionsProvider = new Func<IServiceProvider, MongoVectorStoreOptions?>(sp => null);

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Null(options);
        }
    }
}
