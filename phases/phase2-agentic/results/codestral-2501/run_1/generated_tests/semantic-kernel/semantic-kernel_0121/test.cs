using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.ImageToText;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_WithModel_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://test-endpoint.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddHuggingFaceImageToText(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();
            Assert.NotNull(imageToTextService);
        }

        [Fact]
        public void AddHuggingFaceImageToText_WithEndpoint_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var endpoint = new Uri("https://test-endpoint.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddHuggingFaceImageToText(endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();
            Assert.NotNull(imageToTextService);
        }

        [Fact]
        public void AddHuggingFaceImageToText_ShouldUseLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://test-endpoint.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            // Act
            serviceCollection.AddHuggingFaceImageToText(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();
            Assert.NotNull(imageToTextService);
            loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
        }
    }
}
