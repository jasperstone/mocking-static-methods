using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_ValidParameters_ServiceCollectionReturned()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "model-id";
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var orgId = "org-id";
            var serviceId = "service-id";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "open-telemetry-source-name";
            Action<Microsoft.Extensions.AI.OpenTelemetryChatClient>? openTelemetryConfig = null;

            // Act
            var result = OpenAIServiceCollectionExtensions.AddOpenAIChatClient(
                services,
                modelId,
                endpoint,
                apiKey,
                orgId,
                serviceId,
                httpClient,
                openTelemetrySourceName,
                openTelemetryConfig);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddOpenAIChatClient_GetService_CalledWithCorrectParameters()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "model-id";
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var orgId = "org-id";
            var serviceId = "service-id";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "open-telemetry-source-name";
            Action<Microsoft.Extensions.AI.OpenTelemetryChatClient>? openTelemetryConfig = null;
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(provider => provider.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act
            OpenAIServiceCollectionExtensions.AddOpenAIChatClient(
                services,
                modelId,
                endpoint,
                apiKey,
                orgId,
                serviceId,
                httpClient,
                openTelemetrySourceName,
                openTelemetryConfig);

            // Assert
            serviceProviderMock.Verify(provider => provider.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
