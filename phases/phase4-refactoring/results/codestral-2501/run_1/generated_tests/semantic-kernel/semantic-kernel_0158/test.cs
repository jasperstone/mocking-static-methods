using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using System;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ShouldThrowInvalidOperationException_WhenNoOllamaApiClientIsRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceCollection.AddOllamaTextEmbeddingGeneration());
            Assert.Equal($"No {nameof(IOllamaApiClient)} implementations found in the service collection.", exception.Message);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ShouldReturnServiceCollection_WhenOllamaApiClientIsRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockOllamaClient = new Mock<OllamaApiClient>();
            serviceCollection.AddSingleton(mockOllamaClient.Object);

            // Act
            var result = serviceCollection.AddOllamaTextEmbeddingGeneration();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ServiceCollection>(result);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ShouldUseRegisteredOllamaApiClient_WhenOllamaApiClientIsRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockOllamaClient = new Mock<OllamaApiClient>();
            serviceCollection.AddSingleton(mockOllamaClient.Object);

            // Act
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var ollamaClient = serviceProvider.GetService<OllamaApiClient>();

            // Assert
            Assert.NotNull(ollamaClient);
            Assert.Same(mockOllamaClient.Object, ollamaClient);
        }
    }
}
