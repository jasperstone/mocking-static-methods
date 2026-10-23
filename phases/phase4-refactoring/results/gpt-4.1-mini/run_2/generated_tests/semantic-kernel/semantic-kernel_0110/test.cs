using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = "test-model";
            string bearerKey = "test-bearer-key";
            string location = "us-central1";
            string projectId = "test-project";

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId);

            // Assert
            Assert.Same(services, result);

            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
            Assert.IsType<VertexAIEmbeddingGenerator>(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = "test-model";
            Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("token");
            string location = "us-central1";
            string projectId = "test-project";

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerTokenProvider,
                location,
                projectId);

            // Assert
            Assert.Same(services, result);

            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
            Assert.IsType<VertexAIEmbeddingGenerator>(embeddingGenerator);
        }
    }
}
