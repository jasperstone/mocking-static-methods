using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_AddsIChatCompletionService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_UsesILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");
            var loggerFactory = Mock.Of<ILoggerFactory>();
            var logger = Mock.Of<ILogger>();

            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<ILogger>(logger);

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_CreatesOllamaApiClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }
    }
}
