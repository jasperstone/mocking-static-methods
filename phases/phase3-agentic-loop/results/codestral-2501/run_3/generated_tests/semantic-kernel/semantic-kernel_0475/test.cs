using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => options);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnNull_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Null(result);
        }
    }
}
