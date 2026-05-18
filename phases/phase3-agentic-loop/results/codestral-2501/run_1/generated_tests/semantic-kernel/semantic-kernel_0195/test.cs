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
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var openAIClient = new Mock<OpenAIClient>();
            var serviceId = "testServiceId";
            var modelId = "testModelId";
            var dimensions = 128;

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, openAIClient.Object, serviceId, dimensions);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceId = "testServiceId";
            var modelId = "testModelId";
            var dimensions = 128;
            var openAIClient = new Mock<OpenAIClient>();
            serviceCollection.AddSingleton(openAIClient.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, serviceId: serviceId, dimensions: dimensions);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ShouldGetLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceId = "testServiceId";
            var modelId = "testModelId";
            var dimensions = 128;
            var openAIClient = new Mock<OpenAIClient>();
            var loggerFactory = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(openAIClient.Object);
            serviceCollection.AddSingleton(loggerFactory.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, serviceId: serviceId, dimensions: dimensions);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
            loggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
        }
    }
}
