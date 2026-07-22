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
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOpenAIClient = new Mock<OpenAIClient>();

            // Register OpenAIClient and ILoggerFactory in the service collection
            services.AddSingleton(mockOpenAIClient.Object);
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            var result = OpenAIServiceCollectionExtensions.AddOpenAITextEmbeddingGeneration(
                services,
                modelId: "test-model",
                openAIClient: mockOpenAIClient.Object,
                serviceId: "test-service",
                dimensions: 5);

            // Build service provider to resolve the service
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the ITextEmbeddingGenerationService to trigger the factory delegate
            var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(result);
            Assert.Same(services, result);
            Assert.NotNull(embeddingService);
            Assert.IsType<OpenAITextEmbeddingGenerationService>(embeddingService);
        }
    }
}
