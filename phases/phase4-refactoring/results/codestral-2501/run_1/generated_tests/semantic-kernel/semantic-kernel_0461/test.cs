using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using System;
using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            var options = new MongoVectorStoreOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOriginalOptionsIfEmbeddingGeneratorIsNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            var options = new MongoVectorStoreOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOriginalOptionsIfEmbeddingGeneratorIsProvided()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            var options = new MongoVectorStoreOptions { EmbeddingGenerator = mockEmbeddingGenerator.Object };

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }
    }
}
