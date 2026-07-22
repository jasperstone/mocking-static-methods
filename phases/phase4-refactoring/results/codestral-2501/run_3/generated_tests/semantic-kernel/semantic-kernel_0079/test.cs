using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Http;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_ShouldRegisterIChatClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var deploymentName = "testDeployment";
            var endpoint = "https://testEndpoint";
            var apiKey = "testApiKey";
            var serviceId = "testServiceId";
            var modelId = "testModelId";
            var apiVersion = "testApiVersion";
            var httpClient = new HttpClient();
            var openTelemetrySourceName = "testSourceName";
            var openTelemetryConfig = new Action<OpenTelemetryChatClient>(client => { });

            // Act
            serviceCollection.AddAzureOpenAIChatClient(
                deploymentName,
                endpoint,
                apiKey,
                serviceId,
                modelId,
                apiVersion,
                httpClient,
                openTelemetrySourceName,
                openTelemetryConfig);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();

            // Assert
            Assert.NotNull(chatClient);
        }
    }
}
