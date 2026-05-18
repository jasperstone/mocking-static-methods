using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_Should_Throw_InvalidOperationException_When_No_OllamaApiClient_Implementations_Found()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetService<ITextEmbeddingGenerationService>());
            Assert.Equal("No IOllamaApiClient implementations found in the service collection.", exception.Message);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_Should_Return_Service_When_OllamaApiClient_Provided()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockOllamaClient = new Mock<OllamaApiClient>();
            serviceCollection.AddSingleton(mockOllamaClient.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
        }
    }
}
