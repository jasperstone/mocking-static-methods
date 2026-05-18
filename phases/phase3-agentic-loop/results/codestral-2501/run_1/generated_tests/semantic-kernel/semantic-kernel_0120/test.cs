using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddHuggingFaceTextEmbeddingGeneration(new Uri("https://api.huggingface.co"), "apiKey");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceImageToText_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddHuggingFaceImageToText("model", new Uri("https://api.huggingface.co"), "apiKey");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<IImageToTextService>();

            Assert.NotNull(service);
        }
    }
}
