using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "fake-api-key";
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddHuggingFaceTextEmbeddingGeneration(endpoint, apiKey, serviceId, httpClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldUseLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "fake-api-key";
            var serviceId = "test-service";
            var httpClient = new HttpClient();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddHuggingFaceTextEmbeddingGeneration(endpoint, apiKey, serviceId, httpClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
        }
    }
}
