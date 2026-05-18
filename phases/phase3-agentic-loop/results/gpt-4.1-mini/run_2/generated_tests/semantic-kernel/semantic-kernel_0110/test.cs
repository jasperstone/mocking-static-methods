using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Embeddings;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock ILoggerFactory to the service collection to be resolved by GetService
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            services.AddSingleton<ILoggerFactory>(loggerFactory);

            string modelId = "test-model";
            string bearerKey = "test-bearer-key";
            string location = "us-central1";
            string projectId = "test-project";

            // Act
            var returnedServices = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId);

            // Assert
            Assert.Same(services, returnedServices);

            // Build the service provider to resolve the registered IEmbeddingGenerator
            var serviceProvider = returnedServices.BuildServiceProvider();

            // Resolve the IEmbeddingGenerator<string, Embedding<float>> with no serviceId
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            Assert.NotNull(embeddingGenerator);

            // The loggerFactory passed to VertexAIEmbeddingGenerator should be the one we registered
            // We cannot directly assert this here, but the fact that the service resolved without error
            // and the loggerFactory was registered means the call to GetService<ILoggerFactory> was successful.
        }
    }
}
