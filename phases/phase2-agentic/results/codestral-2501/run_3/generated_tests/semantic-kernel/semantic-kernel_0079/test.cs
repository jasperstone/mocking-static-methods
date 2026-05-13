using System;
using System.Net.Http;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_ShouldAddKeyedSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "testDeployment";
            var endpoint = "https://testEndpoint";
            var apiKey = "testApiKey";

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldConfigureChatClientCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "testDeployment";
            var endpoint = "https://testEndpoint";
            var apiKey = "testApiKey";
            var serviceId = "testServiceId";
            var modelId = "testModelId";
            var apiVersion = "testApiVersion";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "testSourceName";
            Action<OpenTelemetryChatClient> openTelemetryConfig = (client) => { };

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
            // Additional assertions can be made to verify the configuration of the chat client
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldHandleNullHttpClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "testDeployment";
            var endpoint = "https://testEndpoint";
            var apiKey = "testApiKey";

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, httpClient: null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldHandleNullOpenTelemetryConfig()
        {
            // Arrange
            var services = new ServiceCollection();
            var deploymentName = "testDeployment";
            var endpoint = "https://testEndpoint";
            var apiKey = "testApiKey";

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, openTelemetryConfig: null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }
    }
}
