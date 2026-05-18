using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http;
using Azure.Core;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

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
            var openTelemetryConfig = (Action<Microsoft.SemanticKernel.Connectors.AzureOpenAI.OpenTelemetryChatClient>)(client => { });

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

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldGetLoggerFactoryFromServiceProvider()
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
            var openTelemetryConfig = (Action<Microsoft.SemanticKernel.Connectors.AzureOpenAI.OpenTelemetryChatClient>)(client => { });

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

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
            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
