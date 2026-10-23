using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ImageToText;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_WithModel_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://test-endpoint");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var imageToTextServiceMock = new Mock<IImageToTextService>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(HttpClient))).Returns(httpClient);

            // Act
            serviceCollection.AddHuggingFaceImageToText(model, endpoint, apiKey, serviceId, httpClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();
            Assert.NotNull(imageToTextService);
        }

        [Fact]
        public void AddHuggingFaceImageToText_WithEndpoint_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var endpoint = new Uri("https://test-endpoint");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var imageToTextServiceMock = new Mock<IImageToTextService>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(HttpClient))).Returns(httpClient);

            // Act
            serviceCollection.AddHuggingFaceImageToText(endpoint, apiKey, serviceId, httpClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();
            Assert.NotNull(imageToTextService);
        }

        [Fact]
        public void AddHuggingFaceImageToText_ShouldUseLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://test-endpoint");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var imageToTextServiceMock = new Mock<IImageToTextService>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(HttpClient))).Returns(httpClient);

            // Act
            serviceCollection.AddHuggingFaceImageToText(model, endpoint, apiKey, serviceId, httpClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
            Assert.Same(loggerFactoryMock.Object, loggerFactory);
        }
    }
}
