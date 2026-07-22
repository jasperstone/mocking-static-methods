using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Embeddings;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersServiceAndResolvesLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var bearerKey = "test-bearer-key";
            var location = "us-central1";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service-id";

            // Add a logger factory to the service collection to test GetService call
            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            services.AddSingleton<ILoggerFactory>(loggerFactory);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                new HttpClient());

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);

            // The logger factory should be resolved from the service provider inside the factory delegate
            var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(resolvedLoggerFactory);
            Assert.Equal(loggerFactory, resolvedLoggerFactory);
        }
    }
}
