using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Ollama;
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

            // Register mocks in the service collection
            services.AddSingleton(mockLoggerFactory.Object);
            services.AddSingleton(mockOllamaApiClient.Object);

            // Act
            var updatedServices = services.AddOllamaTextGeneration(ollamaClient: null, serviceId: null);

            // Resolve the ITextGenerationService to trigger the factory delegate
            var serviceProvider = updatedServices.BuildServiceProvider();
            var textGenerationService = serviceProvider.GetService<ITextGenerationService>();

            // Assert
            Assert.NotNull(textGenerationService);
        }

        [Fact]
        public void AddOllamaTextGeneration_ThrowsIfNoOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();

            // No OllamaApiClient registered
            var updatedServices = services.AddOllamaTextGeneration(ollamaClient: null, serviceId: null);

            // Act & Assert
            var serviceProvider = updatedServices.BuildServiceProvider();
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = serviceProvider.GetService<ITextGenerationService>();
            });

            Assert.Contains("No IOllamaApiClient implementations found", ex.Message);
        }
    }
}
