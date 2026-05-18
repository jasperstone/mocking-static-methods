using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            var modelId = "test-model";
            Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("token");
            var location = "us-central1";
            var projectId = "test-project";

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Add ILoggerFactory to the service provider to verify GetService call
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var updatedServices = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerTokenProvider,
                location,
                projectId);

            // Build service provider to resolve the embedding generator
            var serviceProvider = updatedServices.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);

            // The logger factory should be resolved from the service provider
            var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(resolvedLoggerFactory);
            Assert.Same(loggerFactoryMock.Object, resolvedLoggerFactory);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            var modelId = "test-model";
            var bearerKey = "test-bearer-key";
            var location = "us-central1";
            var projectId = "test-project";

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Add ILoggerFactory to the service provider to verify GetService call
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var updatedServices = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId);

            // Build service provider to resolve the embedding generator
            var serviceProvider = updatedServices.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);

            // The logger factory should be resolved from the service provider
            var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(resolvedLoggerFactory);
            Assert.Same(loggerFactoryMock.Object, resolvedLoggerFactory);
        }
    }
}
