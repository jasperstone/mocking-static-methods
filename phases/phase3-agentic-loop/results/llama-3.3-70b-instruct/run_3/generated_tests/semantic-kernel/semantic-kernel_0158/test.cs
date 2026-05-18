using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Connectors.Ollama.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextGeneration_ServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var ollamaApiClientMock = new Mock<OllamaApiClient>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient))).Returns(ollamaApiClientMock.Object);

            // Act
            services.AddOllamaTextGeneration(ollamaApiClientMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.Once);
        }
    }
}
