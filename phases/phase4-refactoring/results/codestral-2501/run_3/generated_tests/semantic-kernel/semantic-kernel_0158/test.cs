using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.Ollama.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var ollamaClientMock = new Mock<IOllamaApiClient>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOllamaApiClient)))
                .Returns(ollamaClientMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddOllamaTextEmbeddingGeneration();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var textEmbeddingGenerationService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            Assert.NotNull(textEmbeddingGenerationService);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ShouldThrowInvalidOperationException_WhenNoOllamaApiClientFound()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOllamaApiClient)))
                .Returns((IOllamaApiClient)null);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceCollection.AddOllamaTextEmbeddingGeneration());
            Assert.Equal("No IOllamaApiClient implementations found in the service collection.", exception.Message);
        }
    }
}
