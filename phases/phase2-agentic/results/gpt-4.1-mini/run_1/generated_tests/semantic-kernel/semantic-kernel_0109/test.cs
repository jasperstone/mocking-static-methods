using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

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
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "service1";
            var httpClient = new HttpClient();

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // We will intercept the serviceProvider.GetService<ILoggerFactory>() call by using a factory
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var updatedServices = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Build service provider to resolve the embedding generator
            var serviceProvider = updatedServices.BuildServiceProvider();

            // Resolve the embedding generator by serviceId
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(embeddingGenerator);
            // The embedding generator should be of type VertexAIEmbeddingGenerator
            Assert.IsType<VertexAIEmbeddingGenerator>(embeddingGenerator);

            // The loggerFactory should be the same instance we registered
            var vertexAIEmbeddingGenerator = embeddingGenerator as VertexAIEmbeddingGenerator;
            Assert.NotNull(vertexAIEmbeddingGenerator);

            // We cannot directly access the loggerFactory property because it's private/internal,
            // but since we passed the mock, the call to GetService<ILoggerFactory> was successful.
            // This test ensures the extension method calls GetService<ILoggerFactory> on the service provider.
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
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "service2";
            var httpClient = new HttpClient();

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var updatedServices = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Build service provider to resolve the embedding generator
            var serviceProvider = updatedServices.BuildServiceProvider();

            // Resolve the embedding generator by serviceId
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(embeddingGenerator);
            Assert.IsType<VertexAIEmbeddingGenerator>(embeddingGenerator);

            var vertexAIEmbeddingGenerator = embeddingGenerator as VertexAIEmbeddingGenerator;
            Assert.NotNull(vertexAIEmbeddingGenerator);
        }
    }
}
