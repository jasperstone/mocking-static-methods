using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Microsoft.Extensions.Logging;

namespace AzureOpenAIServiceCollectionExtensionsTests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_ValidParameters_ServiceCollectionReturned()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                "deploymentName",
                "endpoint",
                "apiKey",
                "serviceId",
                "modelId",
                "apiVersion",
                new HttpClient(),
                "openTelemetrySourceName",
                null);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_NullServices_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                null,
                "deploymentName",
                "endpoint",
                "apiKey",
                "serviceId",
                "modelId",
                "apiVersion",
                new HttpClient(),
                "openTelemetrySourceName",
                null));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_EmptyEndpoint_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                "deploymentName",
                string.Empty,
                "apiKey",
                "serviceId",
                "modelId",
                "apiVersion",
                new HttpClient(),
                "openTelemetrySourceName",
                null));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_EmptyApiKey_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                "deploymentName",
                "endpoint",
                string.Empty,
                "serviceId",
                "modelId",
                "apiVersion",
                new HttpClient(),
                "openTelemetrySourceName",
                null));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_GetServiceCalledOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                "deploymentName",
                "endpoint",
                "apiKey",
                "serviceId",
                "modelId",
                "apiVersion",
                new HttpClient(),
                "openTelemetrySourceName",
                null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
