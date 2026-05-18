using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;

namespace SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_Should_Register_Service_With_ServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-ada-002",
                apiKey: "test-api-key",
                orgId: "org-test",
                serviceId: "test-service",
                dimensions: 128);

            var provider = services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<OpenAITextEmbeddingGenerationService>(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_Should_Register_Service_With_OpenAIClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            var mockOpenAIClient = new Mock<OpenAIClient>();
            services.AddSingleton(mockOpenAIClient.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-ada-002",
                openAIClient: null,
                serviceId: "test-service",
                dimensions: 256);

            var provider = services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<OpenAITextEmbeddingGenerationService>(service);
        }
    }
}
