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
        public void AddOllamaTextEmbeddingGeneration_ShouldThrowInvalidOperationException_WhenNoOllamaApiClientFound()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceCollection.AddOllamaTextEmbeddingGeneration());
            Assert.Equal($"No {nameof(IOllamaApiClient)} implementations found in the service collection.", exception.Message);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ShouldReturnServiceCollection_WhenOllamaApiClientFound()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockOllamaApiClient = new Mock<OllamaApiClient>();
            serviceCollection.AddSingleton(mockOllamaApiClient.Object);

            // Act
            var result = serviceCollection.AddOllamaTextEmbeddingGeneration();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ServiceCollection>(result);
        }
    }
}
