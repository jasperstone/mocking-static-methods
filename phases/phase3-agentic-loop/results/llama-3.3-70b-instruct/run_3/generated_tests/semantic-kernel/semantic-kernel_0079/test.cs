using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_WithValidParameters_AddsServiceToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "deploymentName";
            var endpoint = "endpoint";
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var modelId = "modelId";
            var apiVersion = "apiVersion";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "openTelemetrySourceName";
            Action<Microsoft.Extensions.AI.OpenTelemetryChatClient>? openTelemetryConfig = null;

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);

            // Assert
            Assert.True(services.Any(d => d.ServiceType == typeof(Microsoft.SemanticKernel.IChatClient) && d.ImplementationFactory != null));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithNullServiceId_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "deploymentName";
            var endpoint = "endpoint";
            var apiKey = "apiKey";
            var modelId = "modelId";
            var apiVersion = "apiVersion";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "openTelemetrySourceName";
            Action<Microsoft.Extensions.AI.OpenTelemetryChatClient>? openTelemetryConfig = null;

            // Act and Assert
            Assert.DoesNotThrow(() => services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, null, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithNullModelId_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "deploymentName";
            var endpoint = "endpoint";
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var apiVersion = "apiVersion";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "openTelemetrySourceName";
            Action<Microsoft.Extensions.AI.OpenTelemetryChatClient>? openTelemetryConfig = null;

            // Act and Assert
            Assert.DoesNotThrow(() => services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, null, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithNullApiVersion_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "deploymentName";
            var endpoint = "endpoint";
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var modelId = "modelId";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "openTelemetrySourceName";
            Action<Microsoft.Extensions.AI.OpenTelemetryChatClient>? openTelemetryConfig = null;

            // Act and Assert
            Assert.DoesNotThrow(() => services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, null, httpClient, openTelemetrySourceName, openTelemetryConfig));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithNullHttpClient_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "deploymentName";
            var endpoint = "endpoint";
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var modelId = "modelId";
            var apiVersion = "apiVersion";
            var openTelemetrySourceName = "openTelemetrySourceName";
            Action<Microsoft.Extensions.AI.OpenTelemetryChatClient>? openTelemetryConfig = null;

            // Act and Assert
            Assert.DoesNotThrow(() => services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, null, openTelemetrySourceName, openTelemetryConfig));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithNullOpenTelemetrySourceName_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "deploymentName";
            var endpoint = "endpoint";
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var modelId = "modelId";
            var apiVersion = "apiVersion";
            var httpClient = new HttpClient();
            Action<Microsoft.Extensions.AI.OpenTelemetryChatClient>? openTelemetryConfig = null;

            // Act and Assert
            Assert.DoesNotThrow(() => services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, null, openTelemetryConfig));
        }

        [Fact]
        public void AddAzureOpenAIChatClient_WithNullOpenTelemetryConfig_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "deploymentName";
            var endpoint = "endpoint";
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var modelId = "modelId";
            var apiVersion = "apiVersion";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "openTelemetrySourceName";

            // Act and Assert
            Assert.DoesNotThrow(() => services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, null));
        }
    }
}
