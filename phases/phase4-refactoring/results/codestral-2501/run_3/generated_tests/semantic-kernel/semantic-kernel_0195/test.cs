using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace OpenAIServiceCollectionExtensionsTests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId", openAIClient: null);

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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithLoggerFactory_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            serviceCollection.AddOpenAITextEmbeddingGeneration("modelId", openAIClient: null);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }
    }
}
