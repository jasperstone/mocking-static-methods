using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;

namespace OpenAIServiceCollectionExtensionsTests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockHttpClientProvider = new Mock<HttpClientProvider>();
            var mockOpenAIClient = new Mock<OpenAIClient>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(HttpClientProvider)))
                .Returns(mockHttpClientProvider.Object);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(OpenAIClient)))
                .Returns(mockOpenAIClient.Object);

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId", mockOpenAIClient.Object);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockHttpClientProvider = new Mock<HttpClientProvider>();
            var mockOpenAIClient = new Mock<OpenAIClient>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(HttpClientProvider)))
                .Returns(mockHttpClientProvider.Object);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(OpenAIClient)))
                .Returns(mockOpenAIClient.Object);

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            Assert.NotNull(service);
        }
    }
}
