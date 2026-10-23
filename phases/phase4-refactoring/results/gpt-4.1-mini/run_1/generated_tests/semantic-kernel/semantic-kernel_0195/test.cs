using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.OpenAI.Extensions
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_UsesServiceProviderGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOpenAIClient = new Mock<OpenAIClient>();

            // Register OpenAIClient and ILoggerFactory in the service provider
            services.AddSingleton(mockOpenAIClient.Object);
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            var result = services.AddOpenAITextEmbeddingGeneration(
                modelId: "test-model",
                openAIClient: mockOpenAIClient.Object,
                serviceId: "test-service",
                dimensions: 123);

            // Build service provider to invoke the factory delegate
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the ITextEmbeddingGenerationService from the keyed singleton
            var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.Same(services, result);
            Assert.NotNull(embeddingService);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_UsesServiceProviderGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOpenAIClient = new Mock<OpenAIClient>();

            // Register OpenAIClient and ILoggerFactory in the service provider
            services.AddSingleton(mockOpenAIClient.Object);
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            var result = services.AddOpenAITextEmbeddingGeneration(
                modelId: "test-model",
                openAIClient: null,
                serviceId: "test-service",
                dimensions: 123);

            // Build service provider to invoke the factory delegate
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the ITextEmbeddingGenerationService from the keyed singleton
            var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.Same(services, result);
            Assert.NotNull(embeddingService);
        }
    }
}
