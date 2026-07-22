using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AzureOpenAIServiceCollectionExtensionsTests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_WithValidParameters_ReturnsSameServiceCollectionInstance()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                "deploymentName",
                "https://example.com",
                "apiKey",
                serviceId: "serviceId",
                modelId: "modelId",
                apiVersion: "apiVersion",
                httpClient: null,
                openTelemetrySourceName: "openTelemetrySourceName",
                openTelemetryConfig: null);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithNullServices_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                null,
                "deploymentName",
                "https://example.com",
                "apiKey",
                serviceId: "serviceId",
                modelId: "modelId",
                apiVersion: "apiVersion",
                httpClient: null,
                openTelemetrySourceName: "openTelemetrySourceName",
                openTelemetryConfig: null));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithEmptyEndpoint_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                "deploymentName",
                string.Empty,
                "apiKey",
                serviceId: "serviceId",
                modelId: "modelId",
                apiVersion: "apiVersion",
                httpClient: null,
                openTelemetrySourceName: "openTelemetrySourceName",
                openTelemetryConfig: null));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithEmptyApiKey_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                "deploymentName",
                "https://example.com",
                string.Empty,
                serviceId: "serviceId",
                modelId: "modelId",
                apiVersion: "apiVersion",
                httpClient: null,
                openTelemetrySourceName: "openTelemetrySourceName",
                openTelemetryConfig: null));
        }
    }
}
