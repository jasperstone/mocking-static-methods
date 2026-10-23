using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProviderGetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var ollamaClientMock = new Mock<IOllamaApiClient>();

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaClientMock.Object as OllamaApiClient);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.Once);
        }
    }
}
