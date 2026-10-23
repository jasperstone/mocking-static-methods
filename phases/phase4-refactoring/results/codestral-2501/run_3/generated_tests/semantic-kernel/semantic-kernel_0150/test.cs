using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using System;
using System.Net.Http;

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

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_WithHttpClient_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var httpClient = new HttpClient { BaseAddress = new Uri("https://example.com") };

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_WithLoggerFactory_ShouldUseLogger()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

            Assert.NotNull(chatCompletionService);
            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
