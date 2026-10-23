using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ShouldUseGetServiceForOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOllamaClient = new Mock<OllamaApiClient>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Setup GetService to return the logger factory and OllamaApiClient
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(OllamaApiClient)))
                .Returns(mockOllamaClient.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOllamaApiClient)))
                .Returns(mockOllamaClient.Object);

            // Act
            services.AddOllamaTextEmbeddingGeneration((sp, _) => {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                var ollamaClient = sp.GetService<OllamaApiClient>();
                if (ollamaClient == null)
                {
                    throw new InvalidOperationException("OllamaApiClient not found");
                }
                return new DummyEmbeddingService();
            });

            var provider = services.BuildServiceProvider();

            // Verify that GetService was called for ILoggerFactory and OllamaApiClient
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.AtLeastOnce);
        }

        private class DummyEmbeddingService : ITextEmbeddingGenerationService
        {
            public string GenerateEmbedding(string input) => "dummy";
        }
    }
}
