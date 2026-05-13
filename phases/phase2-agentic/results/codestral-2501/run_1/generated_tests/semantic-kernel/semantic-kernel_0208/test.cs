using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;
using Xunit;

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
            var apiKey = "test-api-key";
            var endpoint = new Uri("https://api.openai.com/v1");

            // Act
            serviceCollection.AddOpenAIChatClient(modelId, endpoint, apiKey);

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
            var apiKey = "test-api-key";
            var endpoint = new Uri("https://api.openai.com/v1");
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddOpenAIChatClient(modelId, endpoint, apiKey, httpClient: httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddOpenAIChatClient_ShouldGetLoggerFactoryFromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var apiKey = "test-api-key";
            var endpoint = new Uri("https://api.openai.com/v1");
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            // Act
            serviceCollection.AddOpenAIChatClient(modelId, endpoint, apiKey);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
            loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
