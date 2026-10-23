using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.Ollama.Extensions
{
    public class OllamaServiceCollectionExtensionsTests
    {
        private class DummyOllamaClient { }

        [Fact]
        public void AddOllamaTextGeneration_WithNullOllamaClient_UsesGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();

            var dummyOllamaClient = new DummyOllamaClient();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            // We cannot mock extension methods like GetKeyedService, so we skip those setups.
            serviceProviderMock.Setup(sp => sp.GetService(typeof(DummyOllamaClient))).Returns(dummyOllamaClient);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(object))).Returns(dummyOllamaClient);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            var resultServices = services.AddOllamaTextGeneration(ollamaClient: null, serviceId: null);

            var builtProvider = resultServices.BuildServiceProvider();

            var textGenerationService = builtProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(textGenerationService);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(DummyOllamaClient)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }

        [Fact]
        public void AddOllamaTextGeneration_ThrowsIfNoOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(null);
            // We cannot mock extension methods like GetKeyedService, so we skip those setups.
            serviceProviderMock.Setup(sp => sp.GetService(typeof(DummyOllamaClient))).Returns((object?)null);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(object))).Throws(new InvalidOperationException());

            services.AddSingleton(serviceProviderMock.Object);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var resultServices = services.AddOllamaTextGeneration(ollamaClient: null, serviceId: null);
                var builtProvider = resultServices.BuildServiceProvider();
                var service = builtProvider.GetService<ITextEmbeddingGenerationService>();
            });

            Assert.Contains("No IOllamaApiClient implementations found", ex.Message);
        }
    }
}
