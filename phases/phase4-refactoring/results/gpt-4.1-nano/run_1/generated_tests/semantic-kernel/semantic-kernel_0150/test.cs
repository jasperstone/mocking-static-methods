using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System;

namespace SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(); // Register logging services
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var resultServices = services.AddOllamaChatCompletion("modelId", new Uri("http://localhost"));

            // Assert
            var provider = resultServices.BuildServiceProvider();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }
    }
}
