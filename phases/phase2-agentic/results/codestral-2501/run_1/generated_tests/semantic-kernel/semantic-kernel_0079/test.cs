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

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldUseLoggerFactory()
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

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
            loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldUseHttpClient()
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

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldUseOpenTelemetry()
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

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldUseApiVersion()
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

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldUseServiceId()
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

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ShouldUseModelId()
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

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey, serviceId, modelId, apiVersion, httpClient, openTelemetrySourceName, openTelemetryConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }
    }
}
