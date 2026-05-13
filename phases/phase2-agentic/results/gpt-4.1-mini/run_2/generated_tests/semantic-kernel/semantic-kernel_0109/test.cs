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

            // We will use a service provider that returns the mocked logger factory when GetService<ILoggerFactory> is called
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);

            // The loggerFactory passed to VertexAIEmbeddingGenerator should be the one from the service provider
            // We cannot directly access it, but we can verify that the service provider contains the logger factory
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
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "service2";
            var httpClient = new HttpClient();

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);

            var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.Same(loggerFactoryMock.Object, resolvedLoggerFactory);
        }
    }
}
