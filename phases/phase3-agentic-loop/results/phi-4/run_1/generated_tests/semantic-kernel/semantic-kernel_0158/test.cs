using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using OllamaSharp;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceRetrievedSuccessfully()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var ollamaClientMock = new Mock<OllamaApiClient>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(OllamaApiClient)))
                .Returns(ollamaClientMock.Object);

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services, null, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.Once);
            Assert.NotNull(result);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_NoServiceFound_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(OllamaApiClient)))
                .Returns((OllamaApiClient)null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services, null, null));

            Assert.Equal("No IOllamaApiClient implementations found in the service collection.", exception.Message);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.Once);
        }
    }
}
