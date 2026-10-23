using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Http;
using Azure.AI.OpenAI;
using Azure.Core;
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

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

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
        public void AddAzureOpenAIChatClient_ShouldUseLoggerFactory()
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

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

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
