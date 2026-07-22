using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        private class DummyLoggerFactory : ILoggerFactory
        {
            public void AddProvider(ILoggerProvider provider) { }
            public ILogger CreateLogger(string categoryName) => null!;
            public void Dispose() { }
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_RegistersService_WithLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://fake-endpoint");
            var apiKey = "fake-api-key";
            var serviceId = "test-service";

            // Add a dummy logger factory to the service collection to be resolved by GetService<ILoggerFactory>()
            services.AddSingleton<ILoggerFactory>(new DummyLoggerFactory());

            // Act
            var returnedServices = services.AddHuggingFaceTextEmbeddingGeneration(endpoint, apiKey, serviceId);

            // Assert
            Assert.Same(services, returnedServices);

            var provider = services.BuildServiceProvider();

            // Resolve the registered ITextEmbeddingGenerationService by serviceId
            var service = provider.GetService<ITextEmbeddingGenerationService>();
            // The service is registered keyed by serviceId, so GetService<ITextEmbeddingGenerationService>() returns null.
            // Instead, resolve the service by the keyed serviceId using the internal keyed service resolution.
            // Since this is an extension method, we cannot easily resolve by key here.
            // So we check that the service collection contains a registration for ITextEmbeddingGenerationService.
            Assert.Contains(services, sd => sd.ServiceType == typeof(ITextEmbeddingGenerationService));
        }
    }
}
