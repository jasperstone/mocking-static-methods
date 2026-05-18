using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Xunit;
using Moq;
using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();

            var serviceProvider = services.BuildServiceProvider();

            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOllamaChatCompletion_ServiceProvider_GetService_ReturnsOllamaApiClient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();

            var serviceProvider = services.BuildServiceProvider();

            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var ollamaClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(ollamaClient);
        }
    }
}
