using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.Ollama.Extensions
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextGeneration_WithUri_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("http://localhost");

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var resultServices = services.AddOllamaTextGeneration(modelId, endpoint);

            // Build service provider and resolve the service to trigger the factory delegate
            var serviceProvider = resultServices.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextGenerationService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<OllamaTextGenerationService>(service);
        }

        [Fact]
        public void AddOllamaTextGeneration_WithHttpClient_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var httpClient = new HttpClient();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var resultServices = services.AddOllamaTextGeneration(modelId, httpClient);

            // Build service provider and resolve the service to trigger the factory delegate
            var serviceProvider = resultServices.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextGenerationService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<OllamaTextGenerationService>(service);
        }

        [Fact]
        public void AddOllamaTextGeneration_WithOllamaClient_ThrowsIfNoService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                var resultServices = services.AddOllamaTextGeneration(null, null);
                var serviceProvider = resultServices.BuildServiceProvider();
                var service = serviceProvider.GetService<ITextGenerationService>();
            });
        }

        [Fact]
        public void AddOllamaChatCompletion_WithUri_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("http://localhost");

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var resultServices = services.AddOllamaChatCompletion(modelId, endpoint);

            // Build service provider and resolve the service to trigger the factory delegate
            var serviceProvider = resultServices.BuildServiceProvider();
            var service = serviceProvider.GetService<IChatCompletionService>();

            // Assert
            Assert.NotNull(service);
        }
    }
}
