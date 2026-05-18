using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Qdrant.Client;
using Microsoft.Extensions.VectorData; // Assuming this is where QdrantVectorStoreOptions is located
using Microsoft.SemanticKernel; // Assuming this is where IEmbeddingGenerator is located

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
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => options);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

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
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => options);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetStoreOptions_WhenOptionsProviderReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => null);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_WhenOptionsAlreadyHaveEmbeddingGenerator_ShouldReturnOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns((IEmbeddingGenerator)null);

            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => options);

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(options.EmbeddingGenerator, result.EmbeddingGenerator);
        }
    }
}
