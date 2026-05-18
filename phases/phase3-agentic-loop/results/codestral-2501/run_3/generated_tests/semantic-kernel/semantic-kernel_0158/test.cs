using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;
using OllamaSharp;

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
                serviceProvider.GetService<OllamaApiClient>());
            Assert.Equal($"No {nameof(IOllamaApiClient)} implementations found in the service collection.", exception.Message);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_Should_Return_Service_When_OllamaApiClient_Implementations_Found()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockOllamaApiClient = new Mock<OllamaApiClient>();
            serviceCollection.AddSingleton(mockOllamaApiClient.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var ollamaClient = serviceProvider.GetService<OllamaApiClient>();

            // Assert
            Assert.NotNull(ollamaClient);
            Assert.Equal(mockOllamaApiClient.Object, ollamaClient);
        }
    }
}
