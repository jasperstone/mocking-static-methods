using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://api.openai.com/v1");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";

            // Act
            serviceCollection.AddOpenAIChatClient(modelId, endpoint, apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddOpenAIChatClient_ShouldUseProvidedHttpClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://api.openai.com/v1");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddOpenAIChatClient(modelId, endpoint, apiKey, serviceId: serviceId, httpClient: httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddOpenAIChatClient_ShouldUseDefaultHttpClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://api.openai.com/v1");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";

            // Act
            serviceCollection.AddOpenAIChatClient(modelId, endpoint, apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddOpenAIChatClient_ShouldUseLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://api.openai.com/v1");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var loggerFactory = new Mock<ILoggerFactory>().Object;

            serviceCollection.AddSingleton(loggerFactory);

            // Act
            serviceCollection.AddOpenAIChatClient(modelId, endpoint, apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }
    }
}
