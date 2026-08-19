using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var optionsProvider = new Func<IServiceProvider, object?>(sp => null);
            var storeOptions = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }
    }
}
