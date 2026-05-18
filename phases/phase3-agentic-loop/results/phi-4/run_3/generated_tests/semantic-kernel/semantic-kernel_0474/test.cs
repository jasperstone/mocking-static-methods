using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;
using Microsoft.Extensions.AI; // Assuming IEmbeddingGenerator is in this namespace

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_WhenEmbeddingGeneratorProvided_ShouldReturnOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGeneratorMock.Object);

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WhenEmbeddingGeneratorNotProvided_ShouldReturnOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetStoreOptions_WhenOptionsAlreadyHaveEmbeddingGenerator_ShouldReturnOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            var options = new QdrantVectorStoreOptions
            {
                EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object
            };

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(options, result);
        }
    }
}
