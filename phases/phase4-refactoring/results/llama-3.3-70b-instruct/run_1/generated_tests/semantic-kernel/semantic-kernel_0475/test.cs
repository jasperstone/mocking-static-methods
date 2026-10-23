using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Qdrant.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task GetCollectionOptions_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public async Task GetStoreOptions_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }
    }
}
