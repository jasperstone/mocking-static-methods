using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using OpenAI;
using System;

namespace OpenAIServiceCollectionExtensionsTests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var openAIClient = new Mock<OpenAIClient>();
            var loggerFactory = new Mock<ILoggerFactory>();

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId", openAIClient.Object);

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
            var openAIClient = new Mock<OpenAIClient>();
            var loggerFactory = new Mock<ILoggerFactory>();

            serviceCollection.AddSingleton(openAIClient.Object);
            serviceCollection.AddSingleton(loggerFactory.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithNullModelId_ShouldThrowArgumentException()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => serviceCollection.AddOpenAITextEmbeddingGeneration(null, new Mock<OpenAIClient>().Object));
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithEmptyModelId_ShouldThrowArgumentException()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => serviceCollection.AddOpenAITextEmbeddingGeneration(string.Empty, new Mock<OpenAIClient>().Object));
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithNullServiceCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            IServiceCollection serviceCollection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => serviceCollection.AddOpenAITextEmbeddingGeneration("modelId", new Mock<OpenAIClient>().Object));
        }
    }
}
