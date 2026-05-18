using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using OpenAI;

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
            var serviceId = "testServiceId";
            var modelId = "testModelId";
            var dimensions = 128;

            // Act
            serviceCollection.AddSingleton(openAIClient.Object);
            serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, openAIClient.Object, serviceId, dimensions);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var openAIClient = new Mock<OpenAIClient>();
            var serviceId = "testServiceId";
            var modelId = "testModelId";
            var dimensions = 128;

            // Act
            serviceCollection.AddSingleton(openAIClient.Object);
            serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, serviceId: serviceId, dimensions: dimensions);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ShouldCallGetServiceOnServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceId = "testServiceId";
            var modelId = "testModelId";
            var dimensions = 128;
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddSingleton(mockServiceProvider.Object);
            serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, serviceId: serviceId, dimensions: dimensions);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
