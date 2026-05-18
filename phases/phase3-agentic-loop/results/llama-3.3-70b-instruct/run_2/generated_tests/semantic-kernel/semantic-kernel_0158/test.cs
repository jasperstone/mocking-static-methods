using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace Connectors.Ollama.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var ollamaClient = new Mock<OllamaApiClient>();
            services.AddSingleton(ollamaClient.Object);

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaClient.Object);

            // Assert
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient))).Returns(ollamaClient.Object);
            var result = services.BuildServiceProvider();
            Assert.NotNull(result.GetService<ITextEmbeddingGenerationService>());
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddOllamaTextEmbeddingGeneration());
        }
    }
}
