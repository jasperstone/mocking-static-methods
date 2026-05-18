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

            // Add a mock ILoggerFactory to the service provider to verify GetService call
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var updatedServices = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerTokenProvider,
                location,
                projectId);

            // Build service provider to resolve services
            var serviceProvider = updatedServices.BuildServiceProvider();

            // Resolve the embedding generator to trigger the factory delegate and the GetService call
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(embeddingGenerator);
            // The loggerFactoryMock should be the same instance returned by GetService
            var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
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

            // Add a mock ILoggerFactory to the service provider to verify GetService call
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var updatedServices = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId);

            // Build service provider to resolve services
            var serviceProvider = updatedServices.BuildServiceProvider();

            // Resolve the embedding generator to trigger the factory delegate and the GetService call
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(embeddingGenerator);
            // The loggerFactoryMock should be the same instance returned by GetService
            var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.Same(loggerFactoryMock.Object, resolvedLoggerFactory);
        }
    }
}
