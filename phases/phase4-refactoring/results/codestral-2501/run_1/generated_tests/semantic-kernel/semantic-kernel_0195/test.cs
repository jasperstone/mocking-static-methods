using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockOpenAIClient = new Mock<OpenAIClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId", mockOpenAIClient.Object);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithLoggerFactory_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockOpenAIClient = new Mock<OpenAIClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId", mockOpenAIClient.Object);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithDimensions_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockOpenAIClient = new Mock<OpenAIClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId", mockOpenAIClient.Object, dimensions: 128);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }
    }
}
