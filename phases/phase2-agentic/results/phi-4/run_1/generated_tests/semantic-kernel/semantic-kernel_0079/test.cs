using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "deploymentName";
            var endpoint = "https://example.com";
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var modelId = "modelId";
            var apiVersion = "apiVersion";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "sourceName";
            var openTelemetryConfig = new Action<OpenTelemetryChatClient>(client => { });

            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            // Act
            AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                deploymentName,
                endpoint,
                apiKey,
                serviceId,
                modelId,
                apiVersion,
                httpClient,
                openTelemetrySourceName,
                openTelemetryConfig);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
