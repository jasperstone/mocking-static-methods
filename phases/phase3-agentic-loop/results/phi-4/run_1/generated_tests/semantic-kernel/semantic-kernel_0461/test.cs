using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData; // Correct namespace for MongoVectorStoreOptions and IEmbeddingGenerator
using Microsoft.SemanticKernel; // Ensure this namespace is correct

namespace Microsoft.Extensions.DependencyInjection // Ensure this namespace is correct
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_WhenEmbeddingGeneratorProvided_ShouldReturnOptionsWithGenerator()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var embeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGenerator.Object);

            var optionsProvider = (IServiceProvider sp) => new MongoVectorStoreOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WhenEmbeddingGeneratorNotProvided_ShouldReturnOriginalOptions()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            var optionsProvider = (IServiceProvider sp) => new MongoVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(optionsProvider(mockServiceProvider.Object).EmbeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WhenOptionsProviderReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(mockServiceProvider.Object, null);

            // Assert
            Assert.Null(result);
        }
    }
}
