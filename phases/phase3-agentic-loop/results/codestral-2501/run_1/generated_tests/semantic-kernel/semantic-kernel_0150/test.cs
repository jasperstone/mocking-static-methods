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
            var serviceId = "test-service";

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProvider.GetKeyedService<IChatCompletionService>(serviceId);
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_WithHttpClient_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var httpClient = new HttpClient { BaseAddress = new Uri("https://example.com") };
            var serviceId = "test-service";

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, httpClient, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProvider.GetKeyedService<IChatCompletionService>(serviceId);
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_ShouldGetLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");
            var serviceId = "test-service";
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();

            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProvider.GetKeyedService<IChatCompletionService>(serviceId);
            Assert.NotNull(chatCompletionService);
            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void AddOllamaChatCompletion_ShouldCallGetService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");
            var serviceId = "test-service";
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
