using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;
using OllamaSharp;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_ShouldRegisterService()
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
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_ShouldRegisterServiceWithLogger()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");
            var serviceId = "test-service";
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_ShouldRegisterServiceWithHttpClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var httpClient = new HttpClient();
            var serviceId = "test-service";

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

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOllamaChatCompletion_ShouldUseOllamaApiClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");
            var serviceId = "test-service";
            var ollamaClientMock = new Mock<OllamaApiClient>(endpoint, modelId);
            serviceCollection.AddSingleton(ollamaClientMock.Object);

            // Act
            serviceCollection.AddOllamaChatCompletion(modelId, endpoint, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }
    }
}
