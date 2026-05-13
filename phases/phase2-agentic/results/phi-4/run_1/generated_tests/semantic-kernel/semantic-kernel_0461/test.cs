using System;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_WhenOptionsProviderIsNull_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator_ReturnsOptions()
        {
            // Arrange
            var options = new MongoVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, sp => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_WhenOptionsProviderReturnsNullAndServiceProviderProvidesEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGenerator);

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WhenBothOptionsProviderAndServiceProviderReturnNullEmbeddingGenerator_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, null);

            // Assert
            Assert.Null(result);
        }
    }
}
