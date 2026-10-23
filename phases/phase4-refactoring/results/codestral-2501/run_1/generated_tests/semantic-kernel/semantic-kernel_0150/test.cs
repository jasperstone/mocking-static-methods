using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_WithEndpoint_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");
            var serviceId = "test-service";
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            var chatClientMock = new Mock<IChatClient>();
            serviceCollection.AddSingleton(chatClientMock.Object);

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_WithHttpClient_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var httpClient = new HttpClient();
            var serviceId = "test-service";
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            var chatClientMock = new Mock<IChatClient>();
            serviceCollection.AddSingleton(chatClientMock.Object);

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, httpClient, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_ShouldUseLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");
            var serviceId = "test-service";
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            var chatClientMock = new Mock<IChatClient>();
            serviceCollection.AddSingleton(chatClientMock.Object);

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
            loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
