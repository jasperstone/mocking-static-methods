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
        public void AddAzureOpenAIChatClient_ShouldRegisterIChatClient()
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
            Action<OpenTelemetryChatClient> openTelemetryConfig = _ => { };

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();

            // Assert
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldUseLoggerFactoryFromServiceProvider()
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
            Action<OpenTelemetryChatClient> openTelemetryConfig = _ => { };

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
