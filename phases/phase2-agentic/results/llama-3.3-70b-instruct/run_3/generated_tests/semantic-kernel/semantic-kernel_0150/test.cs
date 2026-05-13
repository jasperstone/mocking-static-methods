using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_GetServiceLoggerFactory_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaChatCompletion(services, "modelId", new Uri("https://example.com"), "serviceId");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOllamaChatCompletion_GetServiceLoggerFactory_ReturnsNull_WhenNoLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaChatCompletion(services, "modelId", new Uri("https://example.com"), "serviceId");

            // Assert
            Assert.NotNull(result);
            Assert.Null(serviceProvider.GetService<ILoggerFactory>());
        }

        [Fact]
        public void AddOllamaChatCompletion_WithHttpClient_GetServiceLoggerFactory_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaChatCompletion(services, "modelId", new HttpClient(), "serviceId");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOllamaChatCompletion_WithHttpClient_GetServiceLoggerFactory_ReturnsNull_WhenNoLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaChatCompletion(services, "modelId", new HttpClient(), "serviceId");

            // Assert
            Assert.NotNull(result);
            Assert.Null(serviceProvider.GetService<ILoggerFactory>());
        }
    }
}
