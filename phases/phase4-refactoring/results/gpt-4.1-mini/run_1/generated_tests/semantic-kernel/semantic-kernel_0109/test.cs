using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("token");
            var location = "us-central1";
            var projectId = "test-project";

            // Add a mock ILoggerFactory to the service collection to be resolved by GetService
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerTokenProvider,
                location,
                projectId);

            // Assert
            Assert.Same(services, result);

            // Build service provider and resolve the embedding generator
            var serviceProvider = result.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var bearerKey = "test-key";
            var location = "us-central1";
            var projectId = "test-project";

            // Add a mock ILoggerFactory to the service collection to be resolved by GetService
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId);

            // Assert
            Assert.Same(services, result);

            // Build service provider and resolve the embedding generator
            var serviceProvider = result.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }
    }
}
