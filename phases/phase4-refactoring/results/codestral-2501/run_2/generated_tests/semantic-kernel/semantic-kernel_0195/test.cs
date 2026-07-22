using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using OpenAI;

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
            var mockOpenAIClient = new Mock<OpenAIClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            serviceCollection.AddSingleton(mockOpenAIClient.Object);
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ShouldCallGetService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOpenAIClient = new Mock<OpenAIClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(OpenAIClient))).Returns(mockOpenAIClient.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(OpenAIClient)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
