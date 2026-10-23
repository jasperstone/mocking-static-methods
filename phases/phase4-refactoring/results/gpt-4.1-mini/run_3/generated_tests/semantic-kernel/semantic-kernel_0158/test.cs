using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using OllamaSharp;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.Ollama.Extensions
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextGeneration_UsesGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOllamaApiClient = new Mock<OllamaApiClient>(MockBehavior.Strict, new Uri("http://localhost"), "modelId");

            services.AddSingleton(mockLoggerFactory.Object);
            services.AddSingleton(mockOllamaApiClient.Object);

            // Act
            var updatedServices = services.AddOllamaTextGeneration(ollamaClient: null, serviceId: null);
            var serviceProvider = updatedServices.BuildServiceProvider();

            // Resolve the ITextGenerationService to trigger the factory delegate and the GetService calls
            var textGenerationService = serviceProvider.GetService<ITextGenerationService>();

            // Assert
            Assert.NotNull(updatedServices);
            Assert.NotNull(textGenerationService);
        }

        [Fact]
        public void AddOllamaTextGeneration_ThrowsIfNoOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var updatedServices = services.AddOllamaTextGeneration(ollamaClient: null, serviceId: null);
            var serviceProvider = updatedServices.BuildServiceProvider();

            // Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var svc = serviceProvider.GetRequiredService<ITextGenerationService>();
            });

            Assert.Contains("No IOllamaApiClient implementations found", ex.Message);
        }
    }
}
