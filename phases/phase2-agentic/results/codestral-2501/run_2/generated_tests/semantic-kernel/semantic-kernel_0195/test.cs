using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var openAIClient = new OpenAIClient();
            var serviceId = "test-service";
            var dimensions = 128;

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, openAIClient, serviceId, dimensions);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<OpenAITextEmbeddingGenerationService>(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var serviceId = "test-service";
            var dimensions = 128;

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(OpenAIClient))).Returns(new OpenAIClient());
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());

            serviceCollection.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, serviceId: serviceId, dimensions: dimensions);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<OpenAITextEmbeddingGenerationService>(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithNullModelId_ShouldThrowArgumentNullException()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            string modelId = null;
            var openAIClient = new OpenAIClient();
            var serviceId = "test-service";
            var dimensions = 128;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, openAIClient, serviceId, dimensions));
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithEmptyModelId_ShouldThrowArgumentException()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "";
            var openAIClient = new OpenAIClient();
            var serviceId = "test-service";
            var dimensions = 128;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => serviceCollection.AddOpenAITextEmbeddingGeneration(modelId, openAIClient, serviceId, dimensions));
        }
    }
}
