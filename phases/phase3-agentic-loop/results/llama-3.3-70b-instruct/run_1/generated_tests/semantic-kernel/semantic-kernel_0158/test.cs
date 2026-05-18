using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Connectors.Ollama.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var ollamaApiClientMock = new Mock<OllamaApiClient>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient)))
                .Returns(ollamaApiClientMock.Object);

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaApiClientMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.Once);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetServiceCalled_WithServiceId()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var ollamaApiClientMock = new Mock<OllamaApiClient>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient)))
                .Returns(ollamaApiClientMock.Object);

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaApiClientMock.Object, "serviceId");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.Once);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetServiceCalled_WithoutOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var ollamaApiClientMock = new Mock<OllamaApiClient>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient)))
                .Returns((OllamaApiClient)null);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddOllamaTextEmbeddingGeneration());
        }
    }
}
