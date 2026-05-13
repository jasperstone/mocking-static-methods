using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_WhenEmbeddingGeneratorProvided_ShouldReturnOptionsWithGenerator()
        {
            // Arrange
            var options = new QdrantCollectionOptions();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();

            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvided_ShouldReturnOriginalOptions()
        {
            // Arrange
            var options = new QdrantCollectionOptions();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(options, result);
        }

        [Fact]
        public void GetCollectionOptions_WhenOptionsProviderReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Null(result);
        }
    }
}
