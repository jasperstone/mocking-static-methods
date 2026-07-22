using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace TestProject
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextGeneration_ServiceProviderGetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHuggingFaceTextGeneration("model", new System.Uri("https://api-inference.huggingface.co/models/model"), "apiKey", "serviceId", new System.Net.Http.HttpClient());
            var serviceProvider = services.BuildServiceProvider();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            var textGenerationService = serviceProvider.GetService<ITextGenerationService>();

            // Assert
            Assert.NotNull(textGenerationService);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ServiceProviderGetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHuggingFaceTextEmbeddingGeneration("model", new System.Uri("https://api-inference.huggingface.co/models/model"), "apiKey", "serviceId", new System.Net.Http.HttpClient());
            var serviceProvider = services.BuildServiceProvider();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            var textEmbeddingGenerationService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(textEmbeddingGenerationService);
        }

        [Fact]
        public void AddHuggingFaceImageToText_ServiceProviderGetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHuggingFaceImageToText("model", new System.Uri("https://api-inference.huggingface.co/models/model"), "apiKey", "serviceId", new System.Net.Http.HttpClient());
            var serviceProvider = services.BuildServiceProvider();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();

            // Assert
            Assert.NotNull(imageToTextService);
        }
    }
}
