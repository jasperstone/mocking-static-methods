using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new MongoVectorStoreOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            var options = new MongoVectorStoreOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ServiceProviderWithNullOptions_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            MongoVectorStoreOptions? options = null;

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.Null(result);
        }
    }
}
