using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Ollama; // Ensure this is the correct namespace

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WithProvidedClient_UsesProvidedClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaClient = new Mock<OllamaApiClient>().Object;
            var serviceProvider = new Mock<IServiceProvider>();

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaClient);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WithoutProvidedClient_RetrievesFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaClient = new Mock<OllamaApiClient>().Object;
            services.AddSingleton<OllamaApiClient>(ollamaClient);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(OllamaApiClient))).Returns(ollamaClient);

            // Act
            services.AddOllamaTextEmbeddingGeneration(serviceProvider.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_NoClientFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(OllamaApiClient))).Returns((OllamaApiClient)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                services.AddOllamaTextEmbeddingGeneration(serviceProvider.Object);
            });
        }
    }
}
